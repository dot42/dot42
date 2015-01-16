/* Copyright (c) 2010 CodeScales.com
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

/*
 * ====================================================================
 *
 * This class is inspired by org.apache.http.entity.mime.content.FileBody
 * 
 * ====================================================================
 *
 */

using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Dot42.BarDeployLib.Entity.Mime
{
    public class FileBody : Body
    {
        private string m_name;
        private string m_fileName;
        private byte[] m_content;
        private string m_mimeType;

        public FileBody(string name, string fileName, FileInfo fileInfo, string mimeType)
            : this(name, fileName, fileInfo)
        {
            this.m_mimeType = mimeType;
        }

        public FileBody(string name, string fileName, FileInfo fileInfo)
        {
            this.m_name = name;
            this.m_fileName = fileName;
            this.m_content = null;

            if (fileInfo == null)
            {
                this.m_content = new byte[0];
            }
            else
            {
                using (BinaryReader reader = new BinaryReader(fileInfo.OpenRead()))
                {
                    this.m_content = reader.ReadBytes((int)reader.BaseStream.Length);
                }
            }
        }

        public byte[] GetContent(string boundry)
        {
            List<byte> bytes = new List<byte>();
            if (this.m_content.Length == 0 || this.m_mimeType == null || this.m_mimeType.Equals(string.Empty))
            {
                bytes.AddRange(Encoding.ASCII.GetBytes(AddPostParametersFile(this.m_name, this.m_fileName, boundry, "application/octet-stream")));
            }
            else
            {
                bytes.AddRange(Encoding.ASCII.GetBytes(AddPostParametersFile(this.m_name, this.m_fileName, boundry, this.m_mimeType)));
            }
            bytes.AddRange(this.m_content);
            bytes.AddRange(Encoding.ASCII.GetBytes("\r\n"));
            return bytes.ToArray();
        }

        internal static string AddPostParametersFile(string name, string fileName, string boundry, string contentType)
        {
            if (name == null)
            {
                name = string.Empty;
            }
            if (fileName == null)
            {
                fileName = string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            string paramBoundry = "--" + boundry + "\r\n";
            string stringParam = "Content-Disposition: form-data; name=\"";
            string paramEnd = "\"; filename=\"" + fileName + "\"\r\nContent-Type: " + contentType + "\r\n\r\n";
            builder.Append(paramBoundry);
            builder.Append(stringParam + name + paramEnd);
            return builder.ToString();
        }

    }
}
