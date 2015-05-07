using System;
using Android.App;
using Android.Graphics;
using Android.Hardware;using Android.OS;
using Android.Util;using Android.Views;
using Dot42.Manifest;

/*
 * Copyright (C) 2010 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

[assembly: Application("AccelerometerPlay", Icon = "@drawable/icon")]

[assembly: UsesPermission(Android.Manifest.Permission.VIBRATE)]
[assembly: UsesPermission(Android.Manifest.Permission.WAKE_LOCK)]

namespace AccelerometerPlay
{
    /// <summary>
    /// This is an example of using the accelerometer to integrate the device's
    /// acceleration to a position using the Verlet method. This is illustrated with
    /// a very simple particle system comprised of a few iron balls freely moving on
    /// an inclined wooden table. The inclination of the virtual table is controlled
    /// by the device's accelerometer.
    /// </summary>
    [Activity(Label = "@string/app_name", ScreenOrientation = ScreenOrientations.Portrait/*, Theme = "@android:style/Theme.NoTitleBar.FullScreen"*/)]
    public class AccelerometerPlayActivity : Activity
    {

        private SimulationView mSimulationView;
        private SensorManager mSensorManager;
        private PowerManager mPowerManager;
        private IWindowManager mWindowManager;
        private Display mDisplay;
        private PowerManager.WakeLock mWakeLock;

        /// <summary>
        /// Called when the activity is first created. </summary>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Get an instance of the SensorManager
            mSensorManager = (SensorManager)GetSystemService(SENSOR_SERVICE);

            // Get an instance of the PowerManager
            mPowerManager = (PowerManager)GetSystemService(POWER_SERVICE);

            // Get an instance of the WindowManager
            mWindowManager = (IWindowManager)GetSystemService(WINDOW_SERVICE);
            mDisplay = mWindowManager.DefaultDisplay;

            // Create a bright wake lock
            mWakeLock = mPowerManager.NewWakeLock(PowerManager.SCREEN_BRIGHT_WAKE_LOCK, this.GetType().Name);

            // instantiate our simulation view and set it as the activity's content
            mSimulationView = new SimulationView(this);
            SetContentView(mSimulationView);
        }

        protected override void OnResume()
        {
            base.OnResume();
            /*
             * when the activity is resumed, we acquire a wake-lock so that the
             * screen stays on, since the user will likely not be fiddling with the
             * screen or buttons.
             */
            mWakeLock.Acquire();

            // Start the simulation
            mSimulationView.StartSimulation();
        }

        protected override void OnPause()
        {
            base.OnPause();
            /*
             * When the activity is paused, we make sure to stop the simulation,
             * release our sensor resources and wake locks
             */

            // Stop the simulation
            mSimulationView.StopSimulation();

            // and release our wake-lock
            mWakeLock.Release();
        }

        internal sealed class SimulationView : View, ISensorEventListener
        {
            private readonly AccelerometerPlayActivity activity;
            // diameter of the balls in meters
            private const float sBallDiameter = 0.004f;
            private static readonly float sBallDiameter2 = sBallDiameter * sBallDiameter;

            // friction of the virtual table and air
            private const float sFriction = 0.1f;

            private Sensor mAccelerometer;
            private long mLastT;
            private float mLastDeltaT;

            private float mXDpi;
            private float mYDpi;
            private float mMetersToPixelsX;
            private float mMetersToPixelsY;
            private Bitmap mBitmap;
            private Bitmap mWood;
            private float mXOrigin;
            private float mYOrigin;
            private float mSensorX;
            private float mSensorY;
            private long mSensorTimeStamp;
            private long mCpuTimeStamp;
            private float mHorizontalBound;
            private float mVerticalBound;
            private readonly ParticleSystem mParticleSystem;

            /*
             * Each of our particle holds its previous and current position, its
             * acceleration. for added realism each particle has its own friction
             * coefficient.
             */
            internal sealed class Particle
            {
                private readonly SimulationView view;
                internal float mPosX;
                internal float mPosY;
                private float mAccelX;
                private float mAccelY;
                private float mLastPosX;
                private float mLastPosY;
                private readonly float mOneMinusFriction;

                internal Particle(SimulationView view)
                {
                    this.view = view;
                    // make each particle a bit different by randomizing its
                    // coefficient of friction
                    var r = ((float)(new Random(1)).NextDouble() - 0.5f) * 0.2f;
                    mOneMinusFriction = 1.0f - sFriction + r;
                }

                public void ComputePhysics(float sx, float sy, float dT, float dTC)
                {
                    // Force of gravity applied to our virtual object
                    const float m = 1000.0f; // mass of our virtual object
                    float gx = -sx * m;
                    float gy = -sy * m;

                    /*
                     * ·F = mA <=> A = ·F / m We could simplify the code by
                     * completely eliminating "m" (the mass) from all the equations,
                     * but it would hide the concepts from this sample code.
                     */
                    float invm = 1.0f / m;
                    float ax = gx * invm;
                    float ay = gy * invm;

                    /*
                     * Time-corrected Verlet integration The position Verlet
                     * integrator is defined as x(t+Æt) = x(t) + x(t) - x(t-Æt) +
                     * a(t)Ætö2 However, the above equation doesn't handle variable
                     * Æt very well, a time-corrected version is needed: x(t+Æt) =
                     * x(t) + (x(t) - x(t-Æt)) * (Æt/Æt_prev) + a(t)Ætö2 We also add
                     * a simple friction term (f) to the equation: x(t+Æt) = x(t) +
                     * (1-f) * (x(t) - x(t-Æt)) * (Æt/Æt_prev) + a(t)Ætö2
                     */
                    float dTdT = dT * dT;
                    float x = mPosX + mOneMinusFriction * dTC * (mPosX - mLastPosX) + mAccelX * dTdT;
                    float y = mPosY + mOneMinusFriction * dTC * (mPosY - mLastPosY) + mAccelY * dTdT;
                    mLastPosX = mPosX;
                    mLastPosY = mPosY;
                    mPosX = x;
                    mPosY = y;
                    mAccelX = ax;
                    mAccelY = ay;
                }

                /*
                 * Resolving constraints and collisions with the Verlet integrator
                 * can be very simple, we simply need to move a colliding or
                 * constrained particle in such way that the constraint is
                 * satisfied.
                 */
                public void ResolveCollisionWithBounds()
                {
                    float xmax = view.mHorizontalBound;
                    float ymax = view.mVerticalBound;
                    float x = mPosX;
                    float y = mPosY;
                    if (x > xmax)
                    {
                        mPosX = xmax;
                    }
                    else if (x < -xmax)
                    {
                        mPosX = -xmax;
                    }
                    if (y > ymax)
                    {
                        mPosY = ymax;
                    }
                    else if (y < -ymax)
                    {
                        mPosY = -ymax;
                    }
                }
            }

            /*
             * A particle system is just a collection of particles
             */
            internal sealed class ParticleSystem
            {
                private readonly SimulationView view;
                internal const int NUM_PARTICLES = 15;
                private readonly Particle[] mBalls = new Particle[NUM_PARTICLES];

                internal ParticleSystem(SimulationView view)
                {
                    this.view = view;
                    /*
                     * Initially our particles have no speed or acceleration
                     */
                    for (int i = 0; i < mBalls.Length; i++)
                    {
                        mBalls[i] = new Particle(view);
                    }
                }

                /*
                 * Update the position of each particle in the system using the
                 * Verlet integrator.
                 */
                private void UpdatePositions(float sx, float sy, long timestamp)
                {
                    long t = timestamp;
                    if (view.mLastT != 0)
                    {
                        float dT = (float)(t - view.mLastT) * (1.0f / 1000000000.0f);
                        if (view.mLastDeltaT != 0)
                        {
                            float dTC = dT / view.mLastDeltaT;
                            int count = mBalls.Length;
                            for (int i = 0; i < count; i++)
                            {
                                Particle ball = mBalls[i];
                                ball.ComputePhysics(sx, sy, dT, dTC);
                            }
                        }
                        view.mLastDeltaT = dT;
                    }
                    view.mLastT = t;
                }

                /*
                 * Performs one iteration of the simulation. First updating the
                 * position of all the particles and resolving the constraints and
                 * collisions.
                 */
                public void Update(float sx, float sy, long now)
                {
                    // update the system's positions
                    UpdatePositions(sx, sy, now);

                    // We do no more than a limited number of iterations
                    const int NUM_MAX_ITERATIONS = 10;

                    /*
                     * Resolve collisions, each particle is tested against every
                     * other particle for collision. If a collision is detected the
                     * particle is moved away using a virtual spring of infinite
                     * stiffness.
                     */
                    bool more = true;
                    int count = mBalls.Length;
                    for (int k = 0; k < NUM_MAX_ITERATIONS && more; k++)
                    {
                        more = false;
                        for (int i = 0; i < count; i++)
                        {
                            Particle curr = mBalls[i];
                            for (int j = i + 1; j < count; j++)
                            {
                                Particle ball = mBalls[j];
                                float dx = ball.mPosX - curr.mPosX;
                                float dy = ball.mPosY - curr.mPosY;
                                float dd = dx * dx + dy * dy;
                                // Check for collisions
                                if (dd <= sBallDiameter2)
                                {
                                    /*
                                     * add a little bit of entropy, after nothing is
                                     * perfect in the universe.
                                     */
                                    dx += ((float)(new Random(1)).NextDouble() - 0.5f) * 0.0001f;
                                    dy += ((float)(new Random(2)).NextDouble() - 0.5f) * 0.0001f;
                                    dd = dx * dx + dy * dy;
                                    // simulate the spring
                                    float d = (float)Math.Sqrt(dd);
                                    float c = (0.5f * (sBallDiameter - d)) / d;
                                    curr.mPosX -= dx * c;
                                    curr.mPosY -= dy * c;
                                    ball.mPosX += dx * c;
                                    ball.mPosY += dy * c;
                                    more = true;
                                }
                            }
                            /*
                             * Finally make sure the particle doesn't intersects
                             * with the walls.
                             */
                            curr.ResolveCollisionWithBounds();
                        }
                    }
                }

                public int ParticleCount
                {
                    get { return mBalls.Length; }
                }

                public float GetPosX(int i)
                {
                    return mBalls[i].mPosX;
                }

                public float GetPosY(int i)
                {
                    return mBalls[i].mPosY;
                }
            }

            public void StartSimulation()
            {
                /*
                 * It is not necessary to get accelerometer events at a very high
                 * rate, by using a slower rate (SENSOR_DELAY_UI), we get an
                 * automatic low-pass filter, which "extracts" the gravity component
                 * of the acceleration. As an added benefit, we use less power and
                 * CPU resources.
                 */
                activity.mSensorManager.RegisterListener(this, mAccelerometer, SensorManager.SENSOR_DELAY_UI);
            }

            public void StopSimulation()
            {
                activity.mSensorManager.UnregisterListener(this);
            }

            public SimulationView(AccelerometerPlayActivity context)
                : base(context)
            {
                activity = context;
                mParticleSystem = new ParticleSystem(this);
                mAccelerometer = activity.mSensorManager.GetDefaultSensor(Sensor.TYPE_ACCELEROMETER);

                var metrics = new DisplayMetrics();
                activity.WindowManager.DefaultDisplay.GetMetrics(metrics);
                mXDpi = metrics.Xdpi;
                mYDpi = metrics.Ydpi;
                mMetersToPixelsX = mXDpi / 0.0254f;
                mMetersToPixelsY = mYDpi / 0.0254f;

                // rescale the ball so it's about 0.5 cm on screen
                var ball = BitmapFactory.DecodeResource(Resources, R.Drawable.ball);
                var dstWidth = (int)(sBallDiameter * mMetersToPixelsX + 0.5f);
                var dstHeight = (int)(sBallDiameter * mMetersToPixelsY + 0.5f);
                mBitmap = Bitmap.CreateScaledBitmap(ball, dstWidth, dstHeight, true);

                var opts = new BitmapFactory.Options();
                opts.InDither = true;
                opts.InPreferredConfig = Bitmap.Config.RGB_565;
                mWood = BitmapFactory.DecodeResource(Resources, R.Drawable.wood, opts);
            }

            protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
            {
                // compute the origin of the screen relative to the origin of
                // the bitmap
                mXOrigin = (w - mBitmap.Width) * 0.5f;
                mYOrigin = (h - mBitmap.Height) * 0.5f;
                mHorizontalBound = ((w / mMetersToPixelsX - sBallDiameter) * 0.5f);
                mVerticalBound = ((h / mMetersToPixelsY - sBallDiameter) * 0.5f);
            }

            public void OnSensorChanged(SensorEvent @event)
            {
                if (@event.Sensor.Type != Sensor.TYPE_ACCELEROMETER)
                {
                    return;
                }
                /*
                 * record the accelerometer data, the event's timestamp as well as
                 * the current time. The latter is needed so we can calculate the
                 * "present" time during rendering. In this application, we need to
                 * take into account how the screen is rotated with respect to the
                 * sensors (which always return data in a coordinate space aligned
                 * to with the screen in its native orientation).
                 */

                //JAVA TO C# CONVERTER TODO TASK: C# doesn't allow accessing outer class instance members within a nested class:
                switch (activity.mDisplay.Rotation)
                {
                    case Surface.ROTATION_0:
                        mSensorX = @event.Values[0];
                        mSensorY = @event.Values[1];
                        break;
                    case Surface.ROTATION_90:
                        mSensorX = -@event.Values[1];
                        mSensorY = @event.Values[0];
                        break;
                    case Surface.ROTATION_180:
                        mSensorX = -@event.Values[0];
                        mSensorY = -@event.Values[1];
                        break;
                    case Surface.ROTATION_270:
                        mSensorX = @event.Values[1];
                        mSensorY = -@event.Values[0];
                        break;
                }

                mSensorTimeStamp = @event.Timestamp;
                mCpuTimeStamp = Java.Lang.System.NanoTime();
            }

            protected override void OnDraw(Canvas canvas)
            {

                /*
                 * draw the background
                 */

                canvas.DrawBitmap(mWood, 0, 0, null);

                /*
                 * compute the new position of our object, based on accelerometer
                 * data and present time.
                 */

                ParticleSystem particleSystem = mParticleSystem;
                long now = mSensorTimeStamp + (Java.Lang.System.NanoTime() - mCpuTimeStamp);
                float sx = mSensorX;
                float sy = mSensorY;

                particleSystem.Update(sx, sy, now);

                float xc = mXOrigin;
                float yc = mYOrigin;
                float xs = mMetersToPixelsX;
                float ys = mMetersToPixelsY;
                Bitmap bitmap = mBitmap;
                int count = particleSystem.ParticleCount;
                for (int i = 0; i < count; i++)
                {
                    /*
                     * We transform the canvas so that the coordinate system matches
                     * the sensors coordinate system with the origin in the center
                     * of the screen and the unit is the meter.
                     */

                    float x = xc + particleSystem.GetPosX(i) * xs;
                    float y = yc - particleSystem.GetPosY(i) * ys;
                    canvas.DrawBitmap(bitmap, x, y, null);
                }

                // and make sure to redraw asap
                Invalidate();
            }

            public void OnAccuracyChanged(Sensor sensor, int accuracy)
            {
            }
        }
    }

}