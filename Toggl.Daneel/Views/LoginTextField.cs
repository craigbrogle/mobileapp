using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using MvvmCross.Platform.Core;
using MvvmCross.Plugins.Color.iOS;
using Toggl.Foundation.MvvmCross.Helper;
using UIKit;

namespace Toggl.Daneel.Views
{
    [Register(nameof(LoginTextField))]
    public sealed class LoginTextField : UITextField
    {
        public event EventHandler IsFirstResponderChanged;

        private const int textSize = 15;
        private const int textTopOffset = 22;
        private const int underlineHeight = 1;
        private const int bigPlaceholderSize = 15;
        private const int smallPlaceholderSize = 12;
        private const float placeholderAnimationDuration = 0.5f;

        private readonly CGColor placeholderColor
            = Color.Login.TextViewPlaceholder.ToNativeColor().CGColor;
        private readonly CALayer underlineLayer = new CALayer();
        private readonly CATextLayer placeholderLayer = new CATextLayer();

        private bool placeholderDrawn;

        public override string Text
        {
            get => base.Text;
            set
            {
                if (string.IsNullOrEmpty(base.Text))
                    movePlaceholderUp();
                if (string.IsNullOrEmpty(value))
                    movePlaceholderDown();
                base.Text = value;
            }
        }

        public LoginTextField(IntPtr handle) : base(handle) {}

        public LoginTextField(CGRect frame) : base(frame) {}

        public override void AwakeFromNib()
        {
            Layer.AddSublayer(underlineLayer);
            Layer.AddSublayer(placeholderLayer);
            BorderStyle = UITextBorderStyle.None;
            Font = UIFont.SystemFontOfSize(textSize);
            underlineLayer.BackgroundColor = placeholderColor;
            VerticalAlignment = UIControlContentVerticalAlignment.Top;
            DrawPlaceholder(Frame);
            
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            updateBottomLine();
        }

        public override CGRect EditingRect(CGRect forBounds)
            => new CGRect(0, textTopOffset, Frame.Width, Frame.Height - textTopOffset);

        public override CGRect TextRect(CGRect forBounds)
            => EditingRect(forBounds);

        public override void DrawPlaceholder(CGRect rect)
        {
            if (placeholderDrawn) return;

            placeholderLayer.String = Placeholder;
            placeholderLayer.ForegroundColor = placeholderColor;
            placeholderLayer.FontSize = bigPlaceholderSize;
            var frameY = (Frame.Height - bigPlaceholderSize) / 2;
            placeholderLayer.Frame = new CGRect(
                0,
                frameY,
                Frame.Width,
                Frame.Height - frameY
            );
            //For antialiasing
            placeholderLayer.ContentsScale = UIScreen.MainScreen.Scale;
            placeholderDrawn = true;
        }

        public override bool BecomeFirstResponder()
        {
            base.BecomeFirstResponder();

            IsFirstResponderChanged?.Raise(this);

            if (placeholderLayer.Frame.Top != 0)
                movePlaceholderUp();

            return true;
        }

        public override bool ResignFirstResponder()
        {
            base.ResignFirstResponder();

            IsFirstResponderChanged?.Raise(this);

            if (string.IsNullOrEmpty(Text))
                movePlaceholderDown();

            return true;
        }

        private void updateBottomLine()
        {
            underlineLayer.Frame = new CGRect(
                0,
                Frame.Height - underlineHeight,
                Frame.Width,
                underlineHeight
            );
        }

        private void movePlaceholderUp()
        {
            var yOffset = -placeholderLayer.Frame.Top;
            CATransaction.Begin();
            CATransaction.AnimationDuration = placeholderAnimationDuration;
            placeholderLayer.AffineTransform = CGAffineTransform.MakeTranslation(0, yOffset);
            placeholderLayer.FontSize = smallPlaceholderSize;
            CATransaction.Commit();
        }

        private void movePlaceholderDown()
        {
            CATransaction.Begin();
            CATransaction.AnimationDuration = placeholderAnimationDuration;
            placeholderLayer.AffineTransform = CGAffineTransform.MakeIdentity();
            placeholderLayer.FontSize = bigPlaceholderSize;
            CATransaction.Commit();
        }
    }
}
