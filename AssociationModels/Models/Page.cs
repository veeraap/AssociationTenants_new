﻿using System;
using System.Collections.Generic;

namespace AssociationEntities.Models;

public partial class Page
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    public string PageTitle { get; set; } = null!;

    public bool? IsHomePage { get; set; }

    public string? ResourcePath { get; set; }

    public int MenuId { get; set; }

    public int? PaddingWidth { get; set; }

    public DateTime? PublishStartDate { get; set; }

    public DateTime? PublishEndDate { get; set; }

    public virtual CustomMenu Menu { get; set; } = null!;
}
