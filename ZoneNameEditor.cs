namespace WinFormsSerial
{
    public class ZoneNameEditor
    {
        private readonly TextBox _editBox;
        private readonly ListBox _listBox;

        public ZoneNameEditor(ListBox listBox)
        {
            _listBox = listBox;
            _editBox = CreateEditBox();
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
            _editBox.Bounds = new Rectangle(
                _listBox.Left + itemRect.Left,
                _listBox.Top + itemRect.Top,
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

        private void CommitEdit()
        {
            if (!_editBox.Visible || _editBox.Text.Length > Constants.ZONE_NAME_LENGTH) return;

            int index = (int)_editBox.Tag;
            _listBox.Items[index] = _editBox.Text;
            _editBox.Visible = false;
        }

        public Control EditBoxControl => _editBox;
    }
}
