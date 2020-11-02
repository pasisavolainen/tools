using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NStack;
using Terminal.Gui;

namespace dtail
{
    // copied with adaptations from UICatalog
    public class ListDataSource<T> : List<T>,
        IListDataSource
    {
        public ListDataSource(IEnumerable<T> items, Func<T, bool> isMarked)
        {
            int index = 0;
            foreach(var item in items)
            {
                Add(item);
                if (isMarked?.Invoke(item) ?? false)
                    Marks.Add(index);
                index++;
            }
        }

        ISet<int> Marks { get; } = new HashSet<int>();
        public IEnumerable<(T item, int index, bool marked)> GetItemMarkings()
            => this.Select((item, index) => (item, index, Marks.Contains(index)));

        public Func<int, T, string> FormatItem = (index, item) => item.ToString();
        public bool IsMarked(int item) => Marks.Contains(item);

        public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width)
        {
            container.Move (col, line);
            var ritem = this[item];
            var s = FormatItem(item, ritem);
			RenderUstr (driver, s, col, line, width);
        }

        public void SetMark(int item, bool value)
        {
            if (value) Marks.Add(item);
            else Marks.Remove(item);
        }

        public IList ToList() => this;

        // A slightly adapted method from: https://github.com/migueldeicaza/gui.cs/blob/fc1faba7452ccbdf49028ac49f0c9f0f42bbae91/Terminal.Gui/Views/ListView.cs#L433-L461
        private void RenderUstr(ConsoleDriver driver, ustring ustr, int col, int line, int width)
        {
            int used = 0;
            int index = 0;
            while (index < ustr.Length)
            {
                (var rune, var size) = Utf8.DecodeRune(ustr, index, index - ustr.Length);
                var count = Rune.ColumnWidth(rune);
                if (used + count >= width) break;
                driver.AddRune(rune);
                used += count;
                index += size;
            }

            while (used < width)
            {
                driver.AddRune(' ');
                used++;
            }
        }
    }
}