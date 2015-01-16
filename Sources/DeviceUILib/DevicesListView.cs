using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dot42.AdbLib;
using Dot42.BarDeployLib;
using Dot42.Shared.UI;
using Dot42.Utility;
using TallComponents.Common.Extensions;
using TallComponents.Common.Util;

namespace Dot42.DeviceLib.UI
{
    public partial class DevicesListView : UserControl
    {
        /// <summary>
        /// The current device selection has changed.
        /// </summary>
        public event EventHandler SelectedDeviceChanged;

        /// <summary>
        /// Fired when a new device is found.
        /// </summary>
        public event EventHandler<EventArgs<AndroidDevice>> DeviceAdded;

        /// <summary>
        /// Fired when a device has been removed.
        /// </summary>
        public event EventHandler<EventArgs<AndroidDevice>> DeviceRemoved;

        /// <summary>
        /// Fired when the status of a device has changed.
        /// </summary>
        public event EventHandler<EventArgs<AndroidDevice>> DeviceStateChanged;

        /// <summary>
        /// Fired when a list item was activated.
        /// </summary>
        public event EventHandler<EventArgs<DeviceItem>> ItemActivated;

        /// <summary>
        /// Fire when CanRefreshDevices has changed value.
        /// </summary>
        public event EventHandler CanRefreshDevicesChanged;

        private bool blackBerryDevicesConnected;
        private bool refreshBusy;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DevicesListView()
        {
            InitializeComponent();
            imageList.Images.Add(Graphics.Icons24.MobilePhone);
            imageList.Images.Add(Graphics.Icons24.MobilePhoneDisabled);
            imageList.Images.Add(Graphics.Icons24.MobilePhoneInCompatible);
            imageList.Images.Add(Graphics.Icons24.MobilePhoneInCompatibleDisabled);
            imageList.Images.Add(Graphics.Icons24.Emulator);
            imageList.Images.Add(Graphics.Icons24.EmulatorDisabled);
            imageList.Images.Add(Graphics.Icons24.EmulatorInCompatible);
            imageList.Images.Add(Graphics.Icons24.EmulatorInCompatibleDisabled);
            deviceMonitor.DeviceAdded += OnDeviceAdded;
            deviceMonitor.DeviceRemoved += OnDeviceRemoved;
            deviceMonitor.DeviceStateChanged += OnDeviceStateChanged;
        }

        /// <summary>
        /// Gets/sets the context menu strip of the listview.
        /// </summary>
        public new ContextMenuStrip ContextMenuStrip
        {
            get { return tvList.ContextMenuStrip; }
            set { tvList.ContextMenuStrip = value; }
        }

        /// <summary>
        /// Reload the list of emulator items.
        /// </summary>
        private void ReloadList()
        {
            var selected = SelectedDevice;
            tvList.BeginUpdate();
            ListViewItem itemToSelect = null;
            if (Locations.IsBlackBerry)
            {
                tvList.Items.Clear();
                foreach (var device in BlackBerryDevices.Instance)
                {
                    var deviceItem = new BlackBerryDeviceItem(device);
                    tvList.Items.Add(deviceItem);
                    if (((selected != null) && (selected.Serial == device.Serial)) ||
                        (itemToSelect == null))
                    {
                        itemToSelect = deviceItem;
                    }
                    SetIsCompatible(deviceItem);
                }
            }
            cmdStartWizard.Visible = false;
            tvList.EndUpdate();
            if (itemToSelect != null)
            {
                itemToSelect.Selected = true;
            }
        }

        /// <summary>
        /// Can <see cref="RefreshDevices"/> be invoked?
        /// </summary>
        public bool CanRefreshDevices
        {
            get { return !refreshBusy; }
        }

        private delegate void AnimationDelegate();

        /// <summary>
        /// Refresh the device list and restart the device monitor
        /// </summary>
        public void RefreshDevices(bool startMonitorAnyWay = false)
        {
            if (refreshBusy)
                return;

            refreshBusy = true;
            CanRefreshDevicesChanged.Fire(this);            

            var monitorStarted = deviceMonitor.Enabled;
            deviceMonitor.Enabled = false;
            tvList.Items.Clear();
            ReloadList();
            deviceMonitor.Enabled = monitorStarted || startMonitorAnyWay;

            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            var cs = ClientSize;
            refreshProgress.SetBounds((cs.Width - refreshProgress.Width) / 2, (cs.Height - refreshProgress.Height) / 2, refreshProgress.Width, refreshProgress.Height);
            refreshProgress.Visible = true;
            Task.Factory.StartNew(() => {
                // Thread.Sleep(1000);
                /*/
                deviceMonitor.WaitForInitialUpdate();
                /*/
                while (!deviceMonitor.ReceivedInitialUpdate())
                {
                    try
                    {
                        refreshProgress.Invoke((AnimationDelegate)delegate()
                        {
                            if (refreshProgress.Value >= refreshProgress.Maximum)
                            {
                                refreshProgress.Value = refreshProgress.Minimum;
                            }
                            else
                            {
                                refreshProgress.PerformStep();
                            }
                        });
                    }
                    catch (ObjectDisposedException)
                    {
                        // Object could be dispose in other thread if the form is closed.
                        // In that case we stop here.
                        break;
                    }
                }
                //*/
            }).ContinueWith(t => RefreshCompleted(), ui);
        }

        /// <summary>
        /// Initial refresh has completed.
        /// </summary>
        private void RefreshCompleted()
        {
            refreshBusy = false;
            refreshProgress.Visible = false;
            UpdateNoDevicesItem();
            CanRefreshDevicesChanged.Fire(this);
        }

        /// <summary>
        /// Make sure there is a no devices item when needed.
        /// </summary>
        private void UpdateNoDevicesItem()
        {
            var hasDevices = tvList.Items.Count > 0;
            cmdStartWizard.Visible = !hasDevices;
        }

        /// <summary>
        /// A device has been added.
        /// </summary>
        void OnDeviceAdded(object sender, EventArgs<AndroidDevice> e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler<EventArgs<AndroidDevice>>(OnDeviceAdded), sender, e);
            }
            else
            {
                var ui = TaskScheduler.FromCurrentSynchronizationContext();
                var createTask = Task.Factory.StartNew(() => new AndroidDeviceItem(e.Data));
                createTask.ContinueWith(x => {
                    if (x.Status == TaskStatus.RanToCompletion)
                    {
                        RemoveItemsBySerial(e.Data.Serial);
                        var item = new AndroidDeviceItem(e.Data);
                        tvList.Items.Add(item);
                        SetIsCompatible(item);
                        ReloadList();
                        if (UserPreferences.Preferences.PreferredDeviceSerial == item.Serial)
                        {
                            Select(item.Device);
                        }
                        UpdateNoDevicesItem();
                        DeviceAdded.Fire(this, e);
                    }
                }, ui);
            }
        }

        /// <summary>
        /// A device has been removed.
        /// </summary>
        void OnDeviceRemoved(object sender, EventArgs<AndroidDevice> e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler<EventArgs<AndroidDevice>>(OnDeviceRemoved), sender, e);
            }
            else
            {
                RemoveItemsBySerial(e.Data.Serial);
                ReloadList();
                UpdateNoDevicesItem();
                DeviceRemoved.Fire(this, e);
            }
        }

        /// <summary>
        /// A device has a state change.
        /// </summary>
        void OnDeviceStateChanged(object sender, EventArgs<AndroidDevice> e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler<EventArgs<AndroidDevice>>(OnDeviceStateChanged), sender, e);
            }
            else
            {
                RemoveItemsBySerial(e.Data.Serial);
                var item = new AndroidDeviceItem(e.Data);
                tvList.Items.Add(item);
                SetIsCompatible(item);
                ReloadList();
                UpdateNoDevicesItem();
                DeviceStateChanged.Fire(this, e);
            }
        }

        /// <summary>
        /// Remove all items with the given serial.
        /// </summary>
        private void RemoveItemsBySerial(string serial)
        {
            List<ListViewItem> toRemove = new List<ListViewItem>();
            foreach (ListViewItem item in tvList.Items)
            {
                if (item is DeviceItem)
                {
                    toRemove.Add(item);
                }
            }
            foreach (var item in toRemove)
            {
                tvList.Items.Remove(item);
            }
        }

        /// <summary>
        /// Load time initialization
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!DesignMode)
            {
                RefreshDevices(true);
            }
        }

        /// <summary>
        /// Only refresh when visible.
        /// </summary>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (!DesignMode)
            {
                DeviceMonitorEnabled = Visible;
            }
        }

        /// <summary>
        /// Is the device monitor enabled?
        /// </summary>
        public bool DeviceMonitorEnabled
        {
            get
            {
                if (Locations.IsAndroid)
                    return deviceMonitor.Enabled;
                if (Locations.IsBlackBerry)
                    return blackBerryDevicesConnected;
                return false;
            }
            set
            {
                if (Locations.IsAndroid)
                {
                    deviceMonitor.Enabled = value;
                }
                else if (Locations.IsBlackBerry)
                {
                    if (value)
                    {
                        if (!blackBerryDevicesConnected)
                        {
                            BlackBerryDevices.Instance.Changed += OnBlackBerryDevicesChanged;
                            blackBerryDevicesConnected = true;
                        }
                    }
                    else
                    {
                        if (blackBerryDevicesConnected)
                        {
                            BlackBerryDevices.Instance.Changed -= OnBlackBerryDevicesChanged;
                            blackBerryDevicesConnected = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// BlackBerry devices have been added/removed.
        /// </summary>
        private void OnBlackBerryDevicesChanged(object sender, EventArgs eventArgs)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler(OnBlackBerryDevicesChanged), sender, eventArgs);
            }
            else
            {
                ReloadList();
                UpdateNoDevicesItem();
                BlackBerryNotifications.PostUpdateNotification();
            }
        }

        /// <summary>
        /// Selection changed.
        /// </summary>
        private void OnAfterNodeSelect(object sender, EventArgs e)
        {
            OnSelectedIndexChanged(sender, e);
        }

        /// <summary>
        /// Selection changed.
        /// </summary>
        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var selection = SelectedDevice;
                if ((selection != null) && (selection.IsConnected))
                {
                    UserPreferences.Preferences.PreferredDeviceSerial = selection.Serial;
                    UserPreferences.SaveNow();
                }
            }
            catch (Exception ex)
            {
                ErrorLog.DumpError(ex);
            }
            SelectedDeviceChanged.Fire(this);
        }

        /// <summary>
        /// Item was activated
        /// </summary>
        private void OnNodeDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                var selection = (DeviceItem)this.tvList.SelectedItems[0];
                if (selection != null)
                {
                    ItemActivated.Fire(this, new EventArgs<DeviceItem>(selection));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.DumpError(ex);
            }
        }

        /// <summary>
        /// Gets the selected Device or null if there is no selection.
        /// </summary>
        public DeviceItem SelectedDevice
        {
            get
            {
                if (tvList.SelectedItems.Count > 0)
                {
                    var selection = tvList.SelectedItems[0];
                    return (selection != null) ? (DeviceItem)selection : null;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Select the row that holds the given device.
        /// </summary>
        public void Select(IDevice device)
        {
            foreach(DeviceItem item in tvList.Items)
            {
                if (item.Device == device)
                {
                    item.Selected = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Delegate used to check if items are compatible with a specific task.
        /// </summary>
        public Func<DeviceItem, bool> IsCompatibleCheck { get; set; }

        /// <summary>
        /// Set the IsCompatible flag of the given device's item.
        /// </summary>
        private void SetIsCompatible(DeviceItem item)
        {
            var check = IsCompatibleCheck;
            if (check == null)
                return;
            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            var task = Task.Factory.StartNew(() => check(item));
            task.ContinueWith(x => { item.IsCompatible = x.Result; OnSelectedIndexChanged(null, EventArgs.Empty); }, ui);
        }

        /// <summary>
        /// Block updates
        /// </summary>
        public void BeginUpdate()
        {
            tvList.BeginUpdate();
        }

        /// <summary>
        /// Resume updates
        /// </summary>
        public void EndUpdate()
        {
            tvList.EndUpdate();
        }

        /// <summary>
        /// Add the given item
        /// </summary>
        public void Add(DeviceItem item)
        {
            tvList.Items.Add(item);
        }

        /// <summary>
        /// Start the connection wizard.
        /// </summary>
        private void OnStartWizardClick(object sender, EventArgs e)
        {
            try
            {
                Browser.Open(Urls.HelpMeConnect);
            }
            catch (Exception ex)
            {
                var msg = string.Format("Cannot open connection wizard because: {0}", ex.Message);
                MessageBox.Show(this, msg, ParentForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
