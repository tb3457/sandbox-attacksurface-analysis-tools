﻿//  Copyright 2021 Google Inc. All Rights Reserved.
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace NtApiDotNet.Win32.DirectoryService
{
    /// <summary>
    /// Class to represent an directory service extended right queries from the current domain.
    /// </summary>
    public sealed class DirectoryServiceExtendedRight
    {
        private readonly Lazy<IReadOnlyList<DirectoryServiceSchemaClass>> _property_set;
        private readonly Lazy<IReadOnlyList<DirectoryServiceSchemaClass>> _applies_to;

        /// <summary>
        /// The common name of the extended right.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The rights GUID for this extended right.
        /// </summary>
        public Guid RightsId { get; }

        /// <summary>
        /// The list of applies to GUIDs.
        /// </summary>
        public IReadOnlyCollection<DirectoryServiceSchemaClass> AppliesTo => _applies_to.Value;

        /// <summary>
        /// The valid accesses for this extended right.
        /// </summary>
        public DirectoryServiceAccessRights ValidAccesses { get; }

        /// <summary>
        /// Get list of properties if a property set.
        /// </summary>
        public IReadOnlyList<DirectoryServiceSchemaClass> PropertySet => _property_set.Value;

        /// <summary>
        /// True if this a property set extended right.
        /// </summary>

        public bool IsPropertySet => ValidAccesses.HasFlagSet(DirectoryServiceAccessRights.ReadProp | DirectoryServiceAccessRights.WriteProp);

        internal DirectoryServiceExtendedRight(Guid rights_guid, string name, IEnumerable<Guid> applies_to, 
            DirectoryServiceAccessRights valid_accesses, Func<IReadOnlyList<DirectoryServiceSchemaClass>> func)
        {
            RightsId = rights_guid;
            Name = name;
            _applies_to = new Lazy<IReadOnlyList<DirectoryServiceSchemaClass>>(
                () => applies_to.Select(g => DirectoryServiceUtils.GetSchemaClass(g) 
                ?? new DirectoryServiceSchemaClass(g)).ToList().AsReadOnly());
            ValidAccesses = valid_accesses;
            _property_set = new Lazy<IReadOnlyList<DirectoryServiceSchemaClass>>(() => IsPropertySet ? func() 
                : new List<DirectoryServiceSchemaClass>().AsReadOnly());
        }
    }
}