using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TamaSmartApp
{
    public partial class FindChipDialog : Form
    {
        private ListBox chipListBox = null!;
        private TextBox searchTextBox = null!;
        private Button selectButton = null!;
        private Button cancelButton = null!;
        private Label searchLabel = null!;
        private List<ChipInfo> allChips;
        private List<ChipInfo> filteredChips;

        public ChipInfo? SelectedChip { get; private set; }

        public FindChipDialog(List<ChipInfo> chips)
        {
            allChips = chips;
            filteredChips = new List<ChipInfo>(chips);
            InitializeComponent();
            PopulateList();
        }

        private void InitializeComponent()
        {
            this.Text = "Find IC";
            this.Size = new System.Drawing.Size(400, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // Search label
            searchLabel = new Label
            {
                Text = "IC marking contains:",
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(150, 20),
                AutoSize = true
            };
            this.Controls.Add(searchLabel);

            // Search textbox
            searchTextBox = new TextBox
            {
                Location = new System.Drawing.Point(10, 35),
                Size = new System.Drawing.Size(360, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            searchTextBox.TextChanged += SearchTextBox_TextChanged;
            this.Controls.Add(searchTextBox);

            // Chip listbox
            chipListBox = new ListBox
            {
                Location = new System.Drawing.Point(10, 65),
                Size = new System.Drawing.Size(360, 250),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            chipListBox.DoubleClick += ChipListBox_DoubleClick;
            this.Controls.Add(chipListBox);

            // Select button
            selectButton = new Button
            {
                Text = "Select IC",
                Location = new System.Drawing.Point(214, 325),
                Size = new System.Drawing.Size(75, 25),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.OK
            };
            selectButton.Click += SelectButton_Click;
            this.Controls.Add(selectButton);

            // Cancel button
            cancelButton = new Button
            {
                Text = "Cancel",
                Location = new System.Drawing.Point(295, 325),
                Size = new System.Drawing.Size(75, 25),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(cancelButton);

            this.AcceptButton = selectButton;
            this.CancelButton = cancelButton;
        }

        private void PopulateList()
        {
            chipListBox.Items.Clear();
            foreach (var chip in filteredChips)
            {
                string displayText = $"{chip.Name} ({chip.Manufacturer})";
                chipListBox.Items.Add(displayText);
            }

            if (chipListBox.Items.Count > 0)
            {
                chipListBox.SelectedIndex = 0;
            }
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            string searchText = searchTextBox.Text.ToLower();
            
            if (string.IsNullOrWhiteSpace(searchText))
            {
                filteredChips = new List<ChipInfo>(allChips);
            }
            else
            {
                filteredChips = allChips.Where(c =>
                    c.Name.ToLower().Contains(searchText) ||
                    c.Manufacturer.ToLower().Contains(searchText)
                ).ToList();
            }

            PopulateList();
        }

        private void ChipListBox_DoubleClick(object sender, EventArgs e)
        {
            SelectChip();
        }

        private void SelectButton_Click(object sender, EventArgs e)
        {
            SelectChip();
        }

        private void SelectChip()
        {
            if (chipListBox.SelectedIndex >= 0 && chipListBox.SelectedIndex < filteredChips.Count)
            {
                SelectedChip = filteredChips[chipListBox.SelectedIndex];
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
