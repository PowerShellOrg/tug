/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Tug.Messages.ModelBinding;

namespace Tug.Messages
{
    public class GetModuleRequest : DscRequest
    {
        public static readonly HttpMethod VERB = HttpMethod.Get;

        public const string ROUTE = "Modules(ModuleName='{ModuleName}',ModuleVersion='{ModuleVersion}')/ModuleContent";
        
        [FromRoute]
        [Required]
        public string ModuleName
        { get; set; }

        [FromRoute]
        public string ModuleVersion
        { get; set; }
    }

    public class GetModuleResponse : DscResponse
    {
        [ToHeaderAttribute(Name = "Checksum")]
        public string ChecksumHeader
        { get; set; }

        [ToHeader(Name = "ChecksumAlgorithm")]
        public string ChecksumAlgorithmHeader
        { get; set; }

        [ToResult]
        public Stream Module
        { get; set; }
    }
}