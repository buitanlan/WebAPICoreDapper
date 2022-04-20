using System;

namespace WebAPICoreDapper.Data.ViewModels;

public class PermissionViewModel
{
    public Guid RoleId { get; set; }

    public string FunctionId { get; set; }

    public string ActionId { get; set; }
}