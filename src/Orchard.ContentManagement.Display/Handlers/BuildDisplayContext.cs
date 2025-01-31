﻿using Orchard.DisplayManagement;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.ContentManagement.Display.Handlers
{
    public class BuildDisplayContext : BuildShapeContext
    {
        public BuildDisplayContext(IShape model, IContent content, string displayType, string groupId, IShapeFactory shapeFactory)
            : base(model, content, groupId, shapeFactory)
        {
            DisplayType = displayType;
        }

        public string DisplayType { get; private set; }

    }
}
