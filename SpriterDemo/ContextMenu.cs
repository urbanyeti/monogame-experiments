using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpriterDemo
{
    public class ContextMenu
    {
        private Dictionary<MenuPosition, MenuItem> _items = new Dictionary<MenuPosition, MenuItem>();

        public List<MenuItem> Items => _items.Values.ToList();

        public void AddItem(MenuItem item, MenuPosition position)
        {
            _items[position] = item;
        }

        public MenuItem GetItem(MenuPosition position)
        {
            return _items.TryGetValue(position, out var item) ? item : null;
        }
    }

    public enum MenuPosition
    {
        TopLeft,
        Top,
        TopRight,
        Left,
        Center,
        Right,
        BottomLeft,
        Bottom,
        BottomRight
    }
}
