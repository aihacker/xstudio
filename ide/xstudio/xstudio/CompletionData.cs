using System;
using System.Linq;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace xstudio
{
    public class CompletionData : ICompletionData
    {
        public CompletionData(string text, string desc = null)
        {
            Text = text;
            Content = text;
            Description = desc;
        }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs args)
        {
            var line = textArea.Document.GetLineByOffset(completionSegment.Offset);
            var lead = textArea.Document.GetText(line).ToArray();

            var begin = completionSegment.Offset - line.Offset;
            var end = completionSegment.EndOffset - line.Offset;

            while (begin > 0)
            {
                if (char.IsLetterOrDigit(lead[begin - 1]))
                    begin--;
                else
                    break;
            }

            while (end < lead.Length)
            {
                if (char.IsLetterOrDigit(lead[end]))
                    end++;
                else
                    break;
            }

            var segment = new TextSegment
            {
                StartOffset = line.Offset + begin,
                EndOffset = line.Offset + end
            };

            textArea.Document.Replace(segment, Text);
        }

        public ImageSource Image { get; private set; }
        public string Text { get; private set; }
        public object Content { get; private set; }
        public object Description { get; private set; }
        public double Priority { get; private set; }
    }
}