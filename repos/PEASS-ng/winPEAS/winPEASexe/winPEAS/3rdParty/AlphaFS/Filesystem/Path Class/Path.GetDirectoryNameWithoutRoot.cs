/*  Copyright (C) 2008-2018 Peter Palotas, Jeffrey Jangli, Alexandr Normuradov
 *  
 *  Permission is hereby granted, free of charge, to any person obtaining a copy 
 *  of this software and associated documentation files (the "Software"), to deal 
 *  in the Software without restriction, including without limitation the rights 
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 *  copies of the Software, and to permit persons to whom the Software is 
 *  furnished to do so, subject to the following conditions:
 *  
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *  
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
 *  THE SOFTWARE. 
 */

using System.Security;

namespace Alphaleonis.Win32.Filesystem
{
   public static partial class Path
   {
      /// <summary>[AlphaFS] Returns the directory information for the specified path string without the root information, for example: "C:\Windows\system32" returns: "Windows".</summary>
      /// <returns>The <paramref name="path"/>without the file name part and without the root information (if any), or <c>null</c> if <paramref name="path"/> is <c>null</c> or if <paramref name="path"/> denotes a root (such as "\", "C:", or * "\\server\share").</returns>
      /// <param name="path">The path.</param>
      [SecurityCritical]
      public static string GetDirectoryNameWithoutRoot(string path)
      {
         return GetDirectoryNameWithoutRootCore(null, path, PathFormat.RelativePath);
      }


      /// <summary>[AlphaFS] Returns the directory information for the specified path string without the root information, for example: "C:\Windows\system32" returns: "Windows".</summary>
      /// <returns>The <paramref name="path"/>without the file name part and without the root information (if any), or <c>null</c> if <paramref name="path"/> is <c>null</c> or if <paramref name="path"/> denotes a root (such as "\", "C:", or * "\\server\share").</returns>
      /// <param name="path">The path.</param>
      /// <param name="pathFormat">Indicates the format of the path parameter(s).</param>
      [SecurityCritical]
      public static string GetDirectoryNameWithoutRoot(string path, PathFormat pathFormat)
      {
         return GetDirectoryNameWithoutRootCore(null, path, pathFormat);
      }
   }
}
