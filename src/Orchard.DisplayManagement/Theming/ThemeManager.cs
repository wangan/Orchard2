﻿using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Theming
{
    public class ThemeManager : IThemeManager
    {
        private readonly IEnumerable<IThemeSelector> _themeSelectors;
        private readonly IExtensionManager _extensionManager;

        private ExtensionDescriptor _theme;

        public ThemeManager(
            IEnumerable<IThemeSelector> themeSelectors, 
            IExtensionManager extensionManager)
        {
            _themeSelectors = themeSelectors;
            _extensionManager = extensionManager;
        }

        public async Task<ExtensionDescriptor> GetThemeAsync()
        {
            // For performance reason, processes the current theme only once per scope (request).
            // This can't be cached as each request gets a different value.
            if (_theme == null)
            {
                var allThemeResults = await Task.WhenAll(_themeSelectors.Select(async x => await x.GetThemeAsync().ConfigureAwait(false))).ConfigureAwait(false);

                var requestTheme = allThemeResults
                    .Where(x => x != null)
                    .OrderByDescending(x => x.Priority)
                    .ToList();

                if (requestTheme.Count == 0)
                {
                    return null;
                }

                // Try to load the theme to ensure it's present
                foreach (var theme in requestTheme)
                {
                    var t = _extensionManager.GetExtension(theme.ThemeName);
                    if (t != null)
                    {
                        return _theme = t;
                    }
                }

                // No valid theme. Don't save the result right now.
                return null;
            }

            return _theme;
        }
    }
}