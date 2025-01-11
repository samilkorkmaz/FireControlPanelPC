namespace FireControlPanelPC
{
    public class ZoneNameEditor
    {
        private readonly TextBox _editBox;
        private readonly ListBox _listBox;
        private readonly HashSet<int> _editedIndices;

        public ZoneNameEditor(ListBox listBox)
        {
            _listBox = listBox;
            _editBox = CreateEditBox();
            _editedIndices = new HashSet<int>();
            ConfigureListBox();
        }

        private TextBox CreateEditBox()
        {
            var editBox = new TextBox
            {
                Visible = false
            };
            editBox.LostFocus += (s, e) => CommitEdit();
            editBox.KeyPress += HandleKeyPress;
            return editBox;
        }

        private void ConfigureListBox()
        {
            _listBox.MouseDoubleClick += HandleListBoxDoubleClick;
            _listBox.DrawMode = DrawMode.OwnerDrawFixed;
            _listBox.DrawItem += HighlightEditedLines;
        }

        private void HighlightEditedLines(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            // Draw the background
            Color backColor;
            Color textColor;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                backColor = SystemColors.Highlight;
                textColor = SystemColors.HighlightText;
            }
            else
            {
                backColor = _editedIndices.Contains(e.Index) ? Color.Yellow : SystemColors.Window;
                textColor = SystemColors.WindowText;
            }

            using var backBrush = new SolidBrush(backColor);
            e.Graphics.FillRectangle(backBrush, e.Bounds);

            // Draw the item text
            string text = _listBox.Items[e.Index]?.ToString() ?? string.Empty;
            using var textBrush = new SolidBrush(textColor);
            e.Graphics.DrawString(text, e.Font!, textBrush, e.Bounds);

            // Draw the focus rectangle if needed
            if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
            {
                e.DrawFocusRectangle();
            }
        }

        private void HandleKeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return && _editBox.Text.Length <= Constants.ZONE_NAME_LENGTH)
            {
                CommitEdit();
                e.Handled = true;
            }
            else if (e.KeyChar == (char)Keys.Escape)
            {
                _editBox.Visible = false;
                e.Handled = true;
            }
            else if (_editBox.Text.Length >= Constants.ZONE_NAME_LENGTH && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
            }
        }

        private void HandleListBoxDoubleClick(object? sender, MouseEventArgs e)
        {
            int clickedIndex = _listBox.IndexFromPoint(e.Location);
            if (clickedIndex == ListBox.NoMatches) return;

            Rectangle itemRect = _listBox.GetItemRectangle(clickedIndex);

            // Convert the ListBox coordinates to screen coordinates
            Point listBoxScreenPoint = _listBox.PointToScreen(new Point(itemRect.Left, itemRect.Top));

            // Get the parent form (group box) with null check
            Form? parentForm = _listBox.FindForm();
            if (parentForm == null) return;  // Exit if we can't find the parent form

            // Convert screen coordinates to the parent form coordinates
            Point locationInForm = parentForm.PointToClient(listBoxScreenPoint);

            _editBox.Bounds = new Rectangle(
                locationInForm.X,
                locationInForm.Y,
                itemRect.Width,
                itemRect.Height
            );

            _editBox.Text = _listBox.Items[clickedIndex].ToString();
            _editBox.Tag = clickedIndex;
            _editBox.Visible = true;
            _editBox.BringToFront();
            _editBox.Focus();
            _editBox.SelectAll();
        }

        bool _isChangedByUser = false;
        public bool IsChangedByUser()
        {
            return _isChangedByUser;
        }

        private void CommitEdit()
        {
            if (!_editBox.Visible || _editBox.Text.Length > Constants.ZONE_NAME_LENGTH) return;

            int index = (int)(_editBox.Tag ?? 0); // Returns 0 if Tag is null
            string originalText = _listBox.Items[index].ToString() ?? string.Empty;

            if (_editBox.Text != originalText)
            {
                _listBox.Items[index] = _editBox.Text;
                _editedIndices.Add(index);
                _listBox.Invalidate(GetItemBounds(index));
                _isChangedByUser = true;
            }

            _editBox.Visible = false;
        }

        private Rectangle GetItemBounds(int index)
        {
            return _listBox.GetItemRectangle(index);
        }

        public Control EditBoxControl => _editBox;

        // Method to check if an item has been edited
        public bool IsItemEdited(int index)
        {
            return _editedIndices.Contains(index);
        }

        // Method to clear edit history
        public void ClearEditHistory()
        {
            _editedIndices.Clear();
            _listBox.Invalidate();
            _isChangedByUser = false;
        }
    }
}