using DG.Tweening;
using TMPro;

namespace Utils
{
    public static class TextMeshProExtensions
    {
        /// <summary>Tweens a TextMeshPro's text to the given value.
        /// Also stores the TextMeshPro as the tween's target so it can be used for filtered operations</summary>
        /// <param name="endValue">The end string to tween to</param><param name="duration">The duration of the tween</param>
        /// <param name="richTextEnabled">If TRUE (default), rich text will be interpreted correctly while animated,
        /// otherwise all tags will be considered as normal text</param>
        /// <param name="scrambleMode">The type of scramble mode to use, if any</param>
        /// <param name="scrambleChars">A string containing the characters to use for scrambling.
        /// Use as many characters as possible (minimum 10) because DOTween uses a fast scramble mode which gives better results with more characters.
        /// Leave it to NULL (default) to use default ones</param>
        public static Tweener DOText(this TMP_Text target, string endValue, float duration, bool richTextEnabled = true, ScrambleMode scrambleMode = ScrambleMode.None, string scrambleChars = null)
        {
            return DOTween.To(() => target.text, x => target.text = x, endValue, duration)
                .SetOptions(richTextEnabled, scrambleMode, scrambleChars)
                .SetTarget(target);
        }

        public static Tweener DOTextFast(this TMP_Text target, float duration)
        {
            target.maxVisibleCharacters = 0;
            return DOTween.To(() => target.maxVisibleCharacters, x => target.maxVisibleCharacters = x, target.text.Length, duration)
                .SetTarget(target);
        }
    }
}