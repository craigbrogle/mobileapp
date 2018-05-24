using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Text;
using Toggl.Giskard.Extensions;
using static Android.Support.V7.Widget.RecyclerView;
using static Toggl.Foundation.Resources;

namespace Toggl.Giskard.Views
{
    public sealed class MainRecyclerViewTouchCallback : ItemTouchHelper.SimpleCallback
    {
        private const int left = 0;
        private const int right = 1;

        private readonly int textMargin;
        private readonly int deleteTextWidth;
        private readonly Drawable[] backgrounds;
        private readonly MainRecyclerView recyclerView;
        private readonly string[] text = { Delete, Continue };
        private readonly TextPaint textPaint = new TextPaint();

        public MainRecyclerViewTouchCallback(Context context, MainRecyclerView recyclerView)
            : base(0, ItemTouchHelper.Left | ItemTouchHelper.Right)
        {
            this.recyclerView = recyclerView;

            backgrounds = new[] { Resource.Color.playButtonRed, Resource.Color.playButtonGreen }
                .Select(res => new Color(ContextCompat.GetColor(context, res)))
                .Select(color => new ColorDrawable(color))
                .ToArray();

            textMargin = 16.DpToPixels(context);

            textPaint.Color = Color.White;
            textPaint.TextAlign = Paint.Align.Left;
            textPaint.TextSize = 15.SpToPixels(context);
            textPaint.SetTypeface(Typeface.Create("sans-serif-medium", TypefaceStyle.Normal));

            deleteTextWidth = (int)textPaint.MeasureText(Delete);
        }

        public override int GetSwipeDirs(RecyclerView recyclerView, ViewHolder viewHolder)
        {
            if (viewHolder is MainRecyclerViewLogViewHolder timeEntriesLogViewHolder && timeEntriesLogViewHolder.CanSync)
            {
                return base.GetSwipeDirs(recyclerView, viewHolder);
            }

            return ItemTouchHelper.ActionStateIdle;
        }

        public override bool OnMove(RecyclerView recyclerView, ViewHolder viewHolder, ViewHolder target)
            => false;

        public override void OnSwiped(ViewHolder viewHolder, int direction)
        {
            var swipedPosition = viewHolder.AdapterPosition;

            if (direction == ItemTouchHelper.Right)
            {
                recyclerView.MainRecyclerAdapter.ContinueTimeEntry(swipedPosition);
                recyclerView.MainRecyclerAdapter.NotifyItemChanged(swipedPosition);
            }
            else
            {
                recyclerView.MainRecyclerAdapter.DeleteTimeEntry(swipedPosition);
            }
        }

        public override void OnChildDraw(Canvas c, RecyclerView recyclerView,
                                         ViewHolder viewHolder,
                                         float dX, float dY,
                                         int actionState, bool isCurrentlyActive)
        {
            var isInvalidIndex = viewHolder.AdapterPosition == -1;
            var isTimeEntryCell = viewHolder is MainRecyclerViewLogViewHolder;
            if (isInvalidIndex || !isTimeEntryCell) return;

            var itemView = viewHolder.ItemView;
            var isSwippingRight = dX > 0;

            int direction, leftOffset, rightOffset, textX = 0;

            if (isSwippingRight)
            {
                direction = right;
                textX = textMargin;
                leftOffset = itemView.Left;
                rightOffset = itemView.Left + (int)dX;
            }
            else
            {
                direction = left;
                leftOffset = itemView.Right + (int)dX;
                rightOffset = itemView.Right;
                textX = rightOffset - textMargin - deleteTextWidth;
            }

            var textY = (int)((itemView.Height / 2) - ((textPaint.Descent() + textPaint.Ascent()) / 2)) + itemView.Top;

            backgrounds[direction].SetBounds(leftOffset, itemView.Top, rightOffset, itemView.Bottom);
            backgrounds[direction].Draw(c);
            c.DrawText(text[direction], textX, textY, textPaint);

            base.OnChildDraw(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
        }
    }
}

