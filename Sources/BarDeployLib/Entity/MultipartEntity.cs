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
 * This class is inspired by org.apache.http.entity.mime.MultipartEntity
 * 
 * ====================================================================
 *
 */

using System;
using System.Collections.Generic;
using System.Text;
using Dot42.BarDeployLib.Entity.Mime;

namespace Dot42.BarDeployLib.Entity
{
    public class MultipartEntity : HttpEntity
    {
        private List<Body> m_bodyList = new List<Body>();
        private string m_boundry;
        
        public MultipartEntity()
        {
            GenerateBoundry();
        }

        public void AddBody(Body body)
        {
            this.m_bodyList.Add(body);
        }
        
        public string ContentEncoding
        {
            get { return null; }
            set {}
        }
        
        public string ContentType
        {
            get { return "multipart/form-data; boundary=" + m_boundry; }
            set {}
        }

        public byte[] Content
        {
            get
            {
                List<byte> byteList = new List<byte>();
                foreach (Body body in this.m_bodyList)
                {
                    byteList.AddRange(body.GetContent(this.m_boundry));
                }
                if (this.m_bodyList.Count > 0)
                {
                    byteList.AddRange(Encoding.ASCII.GetBytes(AddPostParametersEnd(this.m_boundry)));
                }
                return byteList.ToArray();
            }
            set {}
        }

        public long ContentLength
        {
            get
            {
                return this.Content.Length;
            }
            set {}
        }

        public bool IsChunked
        {
            get
            {
                return false;
            }
            set {}
        }

        private static string AddPostParametersEnd(string boundry)
        {
            return "--" + boundry + "--\r\n\r\n";
        }

        private static string allowedCharacters = "abcdefghijklmnopqrstuvwxyz1234567890";
        private void GenerateBoundry()
        {
            Random random = new Random(DateTime.Now.Millisecond);
            StringBuilder sb = new StringBuilder();
            sb.Append("---------------------------");
            for (int i = 0; i < 14; i++)
            {
                sb.Append(allowedCharacters[random.Next(allowedCharacters.Length-1)]);
            }
            this.m_boundry = sb.ToString();
        }
    }
}
