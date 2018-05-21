using System.Collections.Generic;
using System.Linq;
using Foundation;
using Toggl.Foundation.Shortcuts;
using UIKit;

namespace Toggl.Daneel
{
    public sealed class ApplicationShortcutCreator : BaseApplicationShortcutCreator
    {
        private readonly IReadOnlyDictionary<ShortcutType, UIApplicationShortcutIcon> icons = new Dictionary<ShortcutType, UIApplicationShortcutIcon>
        {
            { ShortcutType.Reports, UIApplicationShortcutIcon.FromType(UIApplicationShortcutIconType.Time) },
            { ShortcutType.StopTimeEntry, UIApplicationShortcutIcon.FromType(UIApplicationShortcutIconType.Pause) },
            { ShortcutType.StartTimeEntry, UIApplicationShortcutIcon.FromType(UIApplicationShortcutIconType.Play) },
            { ShortcutType.ContinueLastTimeEntry, UIApplicationShortcutIcon.FromType(UIApplicationShortcutIconType.Play) }
        };

        protected override void ClearAllShortCuts()
        {
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                UIApplication.SharedApplication.ShortcutItems = new UIApplicationShortcutItem[0];
            });
        }

        protected override void SetShortcuts(IEnumerable<ApplicationShortcut> shortcuts)
        {
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                UIApplication.SharedApplication.ShortcutItems = shortcuts
                    .Select(createIosShortcut)
                    .ToArray();
            });
        }

        private UIApplicationShortcutItem createIosShortcut(ApplicationShortcut shortcut)
            => new UIApplicationShortcutItem(
                shortcut.Type.ToString(),
                shortcut.Title,
                shortcut.Subtitle,
                icons[shortcut.Type],
                userInfoFor(shortcut)
            );

        private NSDictionary<NSString, NSObject> userInfoFor(ApplicationShortcut shortcut)
            => new NSDictionary<NSString, NSObject>(new NSString(nameof(ApplicationShortcut.Type)), new NSNumber((int)shortcut.Type));
    }
}
