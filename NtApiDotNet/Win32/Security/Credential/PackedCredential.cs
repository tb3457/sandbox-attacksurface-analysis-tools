﻿//  Copyright 2022 Google LLC. All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using NtApiDotNet.Win32.Security.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace NtApiDotNet.Win32.Security.Credential
{
    /// <summary>
    /// A base class to represent a SEC_WINNT_AUTH_PACKED_CREDENTIALS structure.
    /// </summary>
    public abstract class PackedCredential
    {
        /// <summary>
        /// The packed credentials structure.
        /// </summary>
        protected readonly byte[] _credentials;

        /// <summary>
        /// The type of packed credentials.
        /// </summary>
        public Guid CredType { get; }

        /// <summary>
        /// The list of supported security packages.
        /// </summary>
        public IReadOnlyList<uint> PackageList { get; }

        /// <summary>
        /// Convert the packed credentials to an array.
        /// </summary>
        /// <returns>The packed credentials.</returns>
        public byte[] ToArray()
        {
            return null;
        }

        /// <summary>
        /// Try and parse a packed credential buffer.
        /// </summary>
        /// <param name="data">The data to parse.</param>
        /// <param name="packed_credential">The packed credential.</param>
        /// <returns>True if the packed credential was valid.</returns>
        public static bool TryParse(byte[] data, out PackedCredential packed_credential)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            packed_credential = null;
            if (data.Length < Marshal.SizeOf(typeof(SEC_WINNT_AUTH_PACKED_CREDENTIALS)))
                return false;

            using (var buffer = data.ToBuffer())
            {
                var cred = buffer.Read<SEC_WINNT_AUTH_PACKED_CREDENTIALS>(0);
                if (cred.AuthData.CredType == PackedCredentialTypes.Password)
                {
                    packed_credential = new PackedCredentialPassword(cred.AuthData.CredData.ReadBytes(buffer));
                }
                else
                {
                    packed_credential = new PackedCredentialUnknown(cred.AuthData.CredType, cred.AuthData.CredData.ReadBytes(buffer));
                }
                return true;
            }
        }

        /// <summary>
        /// Parse a packed credential buffer.
        /// </summary>
        /// <param name="data">The data to parse.</param>
        /// <returns>The packed credential.</returns>
        public static PackedCredential Parse(byte[] data)
        {
            if (!TryParse(data, out PackedCredential packed_credential))
                throw new InvalidDataException("Invalid packed credential data.");
            return packed_credential;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cred_type">The type of packed credentials.</param>
        /// <param name="credentials">The packed credentials structure.</param>
        /// <param name="package_list">The list of supported security packages.</param>
        protected PackedCredential(Guid cred_type, byte[] credentials, IEnumerable<uint> package_list)
        {
            CredType = cred_type;
            _credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
            PackageList = (package_list?.ToList() ?? new List<uint>()).AsReadOnly();
        }
    }
}