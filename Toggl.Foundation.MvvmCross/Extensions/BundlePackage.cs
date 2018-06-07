using System;
using MvvmCross.Core.ViewModels;
using System.Linq.Expressions;

namespace Toggl.Foundation.MvvmCross.Extensions
{
    internal sealed class BundlePackage<TViewModel>
    {
        private readonly IMvxBundle bundle;
        private readonly TViewModel viewModel;

        public BundlePackage(IMvxBundle bundle, TViewModel viewModel)
        {
            this.bundle = bundle;
            this.viewModel = viewModel;
        }

        public void Store<TProp>(Expression<Func<TViewModel, TProp>> propertyExpression, object valueOverride = null)
        {
            var name = getName(propertyExpression);

            bundle.Data[name] = valueOverride != null
                ? valueOverride.ToString()
                : propertyExpression.Compile()(viewModel).ToString();
        }

        public bool TryGetValue<TProp, TValue>(Expression<Func<TViewModel, TProp>> propertyExpression, out TValue value)
        {
            value = default(TValue);

            var name = getName(propertyExpression);

            if (!bundle.Data.TryGetValue(name, out var bundleValueString))
                return false;

            var type = typeof(TValue);

            if (type == typeof(string))
            {
                value = (TValue)Convert.ChangeType(bundleValueString, typeof(TValue));
                return true;
            }

            if (type == typeof(int) && int.TryParse(bundleValueString, out var resultInt))
            {
                value = (TValue)Convert.ChangeType(resultInt, typeof(TValue));
                return true;
            }

            if (type == typeof(long) && long.TryParse(bundleValueString, out var resultLong))
            {
                value = (TValue)Convert.ChangeType(resultLong, typeof(TValue));
                return true;
            }

            if (type == typeof(DateTimeOffset) && DateTimeOffset.TryParse(bundleValueString, out var resultDateTimeOffset))
            {
                value = (TValue)Convert.ChangeType(resultDateTimeOffset, typeof(TValue));
                return true;
            }

            if (type == typeof(bool) && bool.TryParse(bundleValueString, out var resultBool))
            {
                value = (TValue)Convert.ChangeType(resultBool, typeof(TValue));
                return true;
            }

            return false;
        }

        private string getName<TProp>(Expression<Func<TViewModel, TProp>> propertyExpression)
        {
            var name = propertyExpression.Body.ToString();
            var firstIndexOfDot = name.IndexOf('.');
            return name.Substring(firstIndexOfDot + 1);
        }
    }
}
