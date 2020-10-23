using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace dtail
{
    class LogView : View
    {
        List<Label> Labels { get; } = new List<Label>();
        public ContainerLogsView ContainerLogsView { get;  }
        public LogView (ContainerLogsView clv)
        {
            ContainerLogsView = clv;
            KeyPress += (e) => OnKeyDown2(e);
            ContainerLogsView.LineArrived += (sender, e) =>
            {
                // we don't know which thread this is on, so lets go into UI via mainloop
                Application.MainLoop.Invoke(() => ContainerLogsView_LineArrived(sender, e));
            };
        }

        private void ContainerLogsView_LineArrived(object sender, LogLine e)
        {
            var labels = Labels.Reverse<Label>();
            var ll = ContainerLogsView.LogLines.Last;
            var conts = ContainerLogsView.Containers;
            foreach(var label in labels)
            {
                if (ll == null)
                    return;
                var line = ll.Value;
                var shortname = conts[line.ContainerId].ShortName ?? line.ContainerId;
                label.Text = $"{line.Time:HH:mm:ss} <{shortname}> {line.Line}";
                ll = ll.Previous;
            }
        }

        private void OnKeyDown2(KeyEventEventArgs e)
        {
            this.OnKeyDown(e.KeyEvent);
        }

        public override void OnDrawContent(Rect viewport)
        {
            var morelabels = Labels.Count;
            for(int y = morelabels; y < viewport.Height; y++)
            {
                var label = new Label($"~")
                {
                    Y = y,
                    Width = Dim.Fill(),
                };
                Labels.Add(label);
                Add(label);
            }
            var overflow = Labels.Count - viewport.Height;
            while(overflow-- > 0)
            {
                var lbl = Labels.Last();
                Labels.Remove(lbl);
                Remove(lbl);
            }
        }
        public override bool OnKeyDown(KeyEvent keyEvent)
            => keyEvent.Key switch
            {
                Key.CursorUp   => true,
                Key.CursorDown => true,
                             _ => base.OnKeyDown(keyEvent),
            };

        public override bool CanFocus { get => true; set => _ = value; }
    }
}