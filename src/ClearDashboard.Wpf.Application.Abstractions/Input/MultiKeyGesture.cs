using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace ClearDashboard.Wpf.Application.Input;

[TypeConverter(typeof(MultiKeyGestureConverter))]
public class MultiKeyGesture : KeyGesture
{
    private readonly IList<Key> _keys;
    private readonly ReadOnlyCollection<Key> _readOnlyKeys;
    private readonly bool _sourceIsSystem;
    private int _currentKeyIndex;
    private DateTime _lastKeyPress;
    private static readonly TimeSpan _maximumDelayBetweenKeyPresses = TimeSpan.FromSeconds(1);

    public MultiKeyGesture(IEnumerable<Key> keys, ModifierKeys modifiers)
        : this(keys, modifiers, string.Empty)
    {
    }

    public MultiKeyGesture(IEnumerable<Key> keys, ModifierKeys modifiers, string displayString)
        : base(Key.None, modifiers, displayString)
    {
        _keys = new List<Key>(keys);
        _readOnlyKeys = new ReadOnlyCollection<Key>(_keys);
        _sourceIsSystem = (modifiers & ModifierKeys.Alt) != 0;

        if (_keys.Count == 0)
        {
            throw new ArgumentException("At least one key must be specified.", nameof(keys));
        }
    }

    public ICollection<Key> Keys => _readOnlyKeys;

    public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
    {
        if (inputEventArgs is not KeyEventArgs args)
        {
            return false;
        }
        var curKey = _sourceIsSystem ? args.SystemKey : args.Key;

        if (!IsDefinedKey(curKey))
        {
            return false;
        }

        if (_currentKeyIndex != 0 && ((DateTime.Now - _lastKeyPress) > _maximumDelayBetweenKeyPresses))
        {
            //took too long to press next key so reset
            _currentKeyIndex = 0;
            return false;
        }

        //the modifier only needs to be held down for the first keystroke, but you could also require that the modifier be held down for every keystroke
        if (_currentKeyIndex == 0 && Modifiers != Keyboard.Modifiers)
        {
            //wrong modifiers
            _currentKeyIndex = 0;
            return false;
        }

        if (_keys[_currentKeyIndex] != curKey)
        {
            //wrong key
            _currentKeyIndex = 0;
            return false;
        }

        ++_currentKeyIndex;

        if (_currentKeyIndex != _keys.Count)
        {
            //still matching
            _lastKeyPress = DateTime.Now;
            inputEventArgs.Handled = true;
            return false;
        }

        //match complete
        _currentKeyIndex = 0;
        return true;
    }

    private static bool IsDefinedKey(Key key)
    {
        return key is >= Key.None and <= Key.OemClear;
    }
}

///// <summary>
/////   Class used to define a multi-key gesture.
///// </summary>
//[TypeConverter(typeof(MultiKeyGestureConverter))]
//public class MultiKeyGesture : InputGesture
//{
//    /// <summary>
//    ///   The maximum delay between key presses.
//    /// </summary>
//    static readonly TimeSpan maximumDelay = TimeSpan.FromSeconds(1);

//    /// <summary>
//    ///   Determines whether the key is define.
//    /// </summary>
//    /// <param name="key"> The key to check. </param>
//    /// <returns> <c>True</c> if the key is defined as a gesture key; otherwise, <c>false</c> . </returns>
//    static bool IsDefinedKey(Key key)
//    {
//        return ((key >= Key.None) && (key <= Key.OemClear));
//    }

//    /// <summary>
//    ///   Gets the key sequences string.
//    /// </summary>
//    /// <param name="sequences"> The key sequences. </param>
//    /// <returns> The string representing the key sequences. </returns>
//    static string GetKeySequencesString(params KeySequence[] sequences)
//    {
//        if (sequences == null)
//            throw new ArgumentNullException("sequences");
//        if (sequences.Length == 0)
//            throw new ArgumentException("At least one sequence must be specified.", "sequences");

//        var builder = new StringBuilder();

//        builder.Append(sequences[0].ToString());

//        for (var i = 1; i < sequences.Length; i++)
//            builder.Append(", " + sequences[i]);

//        return builder.ToString();
//    }

//    /// <summary>
//    ///   Determines whether the specified key is a modifier key.
//    /// </summary>
//    /// <param name="key"> The key. </param>
//    /// <returns> <c>True</c> if the specified key is a modifier key; otherwise, <c>false</c> . </returns>
//    static bool IsModifierKey(Key key)
//    {
//        return key == Key.LeftCtrl || key == Key.RightCtrl || key == Key.LeftShift || key == Key.RightShift || key == Key.LeftAlt || key == Key.RightAlt || key == Key.LWin || key == Key.RWin;
//    }

//    /// <summary>
//    ///   The display string.
//    /// </summary>
//    readonly string displayString;

//    /// <summary>
//    ///   The key sequences composing the gesture.
//    /// </summary>
//    readonly KeySequence[] keySequences;

//    /// <summary>
//    ///   The index of the current gesture key.
//    /// </summary>
//    int currentKeyIndex;

//    /// <summary>
//    ///   The current sequence index.
//    /// </summary>
//    int currentSequenceIndex;

//    /// <summary>
//    ///   The last time a gesture key was pressed.
//    /// </summary>
//    DateTime lastKeyPress;

//    /// <summary>
//    ///   Gets the key sequences composing the gesture.
//    /// </summary>
//    /// <value> The key sequences composing the gesture. </value>
//    public KeySequence[] KeySequences
//    {
//        get { return keySequences; }
//    }

//    /// <summary>
//    ///   Gets the display string.
//    /// </summary>
//    /// <value> The display string. </value>
//    public string DisplayString
//    {
//        get { return displayString; }
//    }

//    /// <summary>
//    ///   Initializes a new instance of the <see cref="MultiKeyGesture" /> class.
//    /// </summary>
//    /// <param name="sequences"> The key sequences. </param>
//    public MultiKeyGesture(params KeySequence[] sequences)
//        : this(GetKeySequencesString(sequences), sequences) { }

//    /// <summary>
//    ///   Initializes a new instance of the <see cref="MultiKeyGesture" /> class.
//    /// </summary>
//    /// <param name="displayString"> The display string. </param>
//    /// <param name="sequences"> The key sequences. </param>
//    public MultiKeyGesture(string displayString, params KeySequence[] sequences)
//    {
//        if (sequences == null)
//            throw new ArgumentNullException("sequences");
//        if (sequences.Length == 0)
//            throw new ArgumentException("At least one sequence must be specified.", "sequences");

//        this.displayString = displayString;
//        keySequences = new KeySequence[sequences.Length];
//        sequences.CopyTo(keySequences, 0);
//    }

//    /// <summary>
//    ///   Determines whether this <see cref="System.Windows.Input.KeyGesture" /> matches the input associated with the specified <see
//    ///    cref="System.Windows.Input.InputEventArgs" /> object.
//    /// </summary>
//    /// <param name="targetElement"> The target. </param>
//    /// <param name="inputEventArgs"> The input event data to compare this gesture to. </param>
//    /// <returns> true if the event data matches this <see cref="System.Windows.Input.KeyGesture" /> ; otherwise, false. </returns>
//    public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
//    {
//        var args = inputEventArgs as KeyEventArgs;

//        if (args == null || args.IsRepeat)
//            return false;

//        var key = args.Key != Key.System ? args.Key : args.SystemKey;
//        //Check if the key identifies a gesture key...
//        if (!IsDefinedKey(key))
//            return false;

//        var currentSequence = keySequences[currentSequenceIndex];
//        var currentKey = currentSequence.Keys[currentKeyIndex];

//        //Check if the key is a modifier...
//        if (IsModifierKey(key))
//        {
//            //If the pressed key is a modifier, ignore it for now, since it is tested afterwards...
//            return false;
//        }

//        //Check if the current key press happened too late...
//        if (currentSequenceIndex != 0 && ((DateTime.Now - lastKeyPress) > maximumDelay))
//        {
//            //The delay has expired, abort the match...
//            ResetState();
//#if DEBUG_MESSAGES
//                System.Diagnostics.Debug.WriteLine("Maximum delay has elapsed", "[" + MultiKeyGestureConverter.Default.ConvertToString(this) + "]");
//#endif
//            return false;
//        }

//        //Check if current modifiers match required ones...
//        if (currentSequence.Modifiers != args.KeyboardDevice.Modifiers)
//        {
//            //The modifiers are not the expected ones, abort the match...
//            ResetState();
//#if DEBUG_MESSAGES
//                System.Diagnostics.Debug.WriteLine("Incorrect modifier " + args.KeyboardDevice.Modifiers + ", expecting " + currentSequence.Modifiers, "[" + MultiKeyGestureConverter.Default.ConvertToString(this) + "]");
//#endif
//            return false;
//        }

//        //Check if the current key is not correct...
//        if (currentKey != key)
//        {
//            //The current key is not correct, abort the match...
//            ResetState();
//#if DEBUG_MESSAGES
//                System.Diagnostics.Debug.WriteLine("Incorrect key " + key + ", expecting " + currentKey, "[" + MultiKeyGestureConverter.Default.ConvertToString(this) + "]");
//#endif
//            return false;
//        }

//        //Move on the index, pointing to the next key...
//        currentKeyIndex++;

//        //Check if the key is the last of the current sequence...
//        if (currentKeyIndex == keySequences[currentSequenceIndex].Keys.Length)
//        {
//            //The key is the last of the current sequence, go to the next sequence...
//            currentSequenceIndex++;
//            currentKeyIndex = 0;
//        }

//        //Check if the sequence is the last one of the gesture...
//        if (currentSequenceIndex != keySequences.Length)
//        {
//            //If the key is not the last one, get the current date time, handle the match event but do nothing...
//            lastKeyPress = DateTime.Now;
//            inputEventArgs.Handled = true;
//#if DEBUG_MESSAGES
//                System.Diagnostics.Debug.WriteLine("Waiting for " + (m_KeySequences.Length - m_CurrentSequenceIndex) + " sequences", "[" + MultiKeyGestureConverter.Default.ConvertToString(this) + "]");
//#endif
//            return false;
//        }

//        //The gesture has finished and was correct, complete the match operation...
//        ResetState();
//        inputEventArgs.Handled = true;
//#if DEBUG_MESSAGES
//            System.Diagnostics.Debug.WriteLine("Gesture completed " + MultiKeyGestureConverter.Default.ConvertToString(this), "[" + MultiKeyGestureConverter.Default.ConvertToString(this) + "]");
//#endif
//        return true;
//    }

//    /// <summary>
//    ///   Resets the state of the gesture.
//    /// </summary>
//    void ResetState()
//    {
//        currentSequenceIndex = 0;
//        currentKeyIndex = 0;
//    }
//}