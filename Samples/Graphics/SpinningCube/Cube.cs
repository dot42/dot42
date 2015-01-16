using Java.Nio;
using Javax.Microedition.Khronos.Opengles;

namespace SpinningCube
{
   /// <summary>
   /// Cube definition plus code to draw itself to OpenGL surface.
   /// </summary>
   internal class Cube
   {
      private FloatBuffer mVertexBuffer;
      private FloatBuffer mColorBuffer;
      private ByteBuffer mIndexBuffer;

      // every 3 entries represent the position of one vertex
      private float[] vertices =
            {
                -1.0f, -1.0f, -1.0f, 
                1.0f, -1.0f, -1.0f, 
                1.0f, 1.0f, -1.0f, 
                -1.0f, 1.0f, -1.0f,
                -1.0f, -1.0f, 1.0f, 
                1.0f, -1.0f, 1.0f, 
                1.0f, 1.0f, 1.0f, 
                -1.0f, 1.0f, 1.0f
            };

      // every 4 entries represent the color (r,g,b,a) 
      // of the corresponding vertex in vertices
      private float[] colors =
            {
                0.0f, 1.0f, 0.0f, 1.0f, 
                0.0f, 1.0f, 0.0f, 1.0f, 
                1.0f, 0.5f, 0.0f, 1.0f, 
                1.0f, 0.5f, 0.0f, 1.0f, 
                1.0f, 0.0f, 0.0f, 1.0f, 
                1.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                1.0f, 0.0f, 1.0f, 1.0f
            };

      private byte[] indices =
            {
                0, 4, 5, 0, 5, 1, // two triangles for side 1
                1, 5, 6, 1, 6, 2, // two triangles for side 2
                2, 6, 7, 2, 7, 3, // etc.
                3, 7, 4, 3, 4, 0, 
                4, 7, 6, 4, 6, 5, 
                3, 0, 1, 3, 1, 2
            };

      public Cube()
      {
         ByteBuffer byteBuf = ByteBuffer.AllocateDirect(vertices.Length * 4);
         byteBuf.Order(ByteOrder.NativeOrder());
         mVertexBuffer = byteBuf.AsFloatBuffer();
         mVertexBuffer.Put(vertices);
         mVertexBuffer.Position(0);

         byteBuf = ByteBuffer.AllocateDirect(colors.Length * 4);
         byteBuf.Order(ByteOrder.NativeOrder());
         mColorBuffer = byteBuf.AsFloatBuffer();
         mColorBuffer.Put(colors);
         mColorBuffer.Position(0);

         mIndexBuffer = ByteBuffer.AllocateDirect(indices.Length);
         mIndexBuffer.Put(indices);
         mIndexBuffer.Position(0);
      }

      public void Draw(IGL10 gl)
      {
         gl.GlFrontFace(IGL10Constants.GL_CW);

         gl.GlVertexPointer(3, IGL10Constants.GL_FLOAT, 0, mVertexBuffer);
         gl.GlColorPointer(4, IGL10Constants.GL_FLOAT, 0, mColorBuffer);

         gl.GlEnableClientState(IGL10Constants.GL_VERTEX_ARRAY);
         gl.GlEnableClientState(IGL10Constants.GL_COLOR_ARRAY);

         // draw all 36 triangles
         gl.GlDrawElements(IGL10Constants.GL_TRIANGLES, 36, IGL10Constants.GL_UNSIGNED_BYTE,
                           mIndexBuffer);

         gl.GlDisableClientState(IGL10Constants.GL_VERTEX_ARRAY);
         gl.GlDisableClientState(IGL10Constants.GL_COLOR_ARRAY);
      }
   }
}