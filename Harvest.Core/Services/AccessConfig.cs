﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Domain;
using Harvest.Core.Models;

namespace Harvest.Core.Services
{
    public class AccessConfig
    {
        // define accessCode => roleCode[] mapping once for both AccessHandler and UserService
        public static string[] GetRoles(string accessCode)
        {
            return accessCode switch
            {
                // System can access anything
                AccessCodes.SystemAccess => new[] { Role.Codes.System },
                // FieldManager can access anything restricted to FieldManager, Supervisor, Worker or PI roles
                AccessCodes.FieldManagerAccess => new[] { Role.Codes.FieldManager },
                // Supervisor can access anything restricted to Supervisor, Worker or PI roles
                AccessCodes.SupervisorAccess => new[] { Role.Codes.Supervisor, Role.Codes.FieldManager },
                // Worker can access anything restricted to Worker role
                AccessCodes.WorkerAccess => new[] { Role.Codes.Worker, Role.Codes.Supervisor, Role.Codes.FieldManager },
                // PI can access anything restricted to PI role
                AccessCodes.PrincipalInvestigator => new[] { Role.Codes.PI, Role.Codes.Supervisor, Role.Codes.FieldManager },
                _ => throw new ArgumentException($"{nameof(accessCode)} is not a valid {nameof(AccessCodes)} constant")
            };
        }
    }
}