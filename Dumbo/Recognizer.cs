using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Speech.Recognition;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

namespace Dumbo
{
    class Recognizer
    {
        public List<string> AllCommands;
        Settings CurrentSettings;
        Choices ChoicesPool;
        Choices NumberedChoices;
        Grammar CommandsGrammar;
        DictionaryLoader Loader;
        public SpeechRecognitionEngine Engine;

        public Recognizer()
        {
            Engine = new SpeechRecognitionEngine();
            Engine.EndSilenceTimeout = new TimeSpan(3000000);
            Engine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);
            Engine.SetInputToDefaultAudioDevice();
            Loader = new DictionaryLoader();
            Loader.LoadDictionaryFiles();
            CurrentSettings = Loader.LoadSettings();
            AddSettingsGrammar();
            LoadCommands();
            LoadCommandsGrammar();
            Engine.RecognizeAsync(RecognizeMode.Multiple);
        }
        
        void LoadCommands()
        {
            ChoicesPool = new Choices();
            NumberedChoices = new Choices();
            AllCommands = new List<string>();
            foreach (var dict in Loader.Dictionaries)
            {
                if (dict == null) continue;
                bool keepGoing = true;
                if (dict.Scopes != null)
                {
                    foreach (var item in dict.Scopes)
                    {
                        if (!CurrentSettings.Scopes.ContainsKey(item.Key) || CurrentSettings.Scopes[item.Key].Current != item.Value)
                        {
                            keepGoing = false;
                            break;
                        }
                    }
                    if (!keepGoing) continue;
                }
                if (dict.Commands != null) AllCommands.AddRange(dict.Commands.Keys);
                if (dict.NumberedCommands != null && dict.NumberedCommands.Count > 0)
                {
                    MakeRepeatedGrammar(dict.NumberedCommands.Keys.ToArray(), Enumerable.Range(0, 10).Select(i => i.ToString()).ToArray(), 99);
                }
                if (dict.DictationCommands != null && dict.DictationCommands.Length > 0)
                {
                    var gb = new GrammarBuilder(new Choices(dict.DictationCommands));
                    gb.AppendDictation();
                    Engine.LoadGrammar(new Grammar(gb));
                }
            }
        }

        public void AddSettingsGrammar()
        {
            CurrentSettings = Loader.LoadSettings();
            var gb = new GrammarBuilder(new Choices(new string[] { CurrentSettings.EnableKeyword, CurrentSettings.DisableKeyword }));
            var scopeChoices = new Choices();
            foreach (var item in CurrentSettings.Scopes)
            {
                var scopeGb = new GrammarBuilder(item.Key);
                var optionsChoices = new Choices(item.Value.Options);
                scopeGb.Append(optionsChoices);
                scopeChoices.Add(scopeGb);
            }
            gb.Append(scopeChoices);
            Engine.LoadGrammar(new Grammar(gb));
        }

        void MakeRepeatedGrammar(string[] firstWords, string[] choicesArr, int choicesMax = 99)
        {
            var gb = new GrammarBuilder(new Choices(firstWords));
            var numChoices = new Choices(choicesArr);
            gb.Append(new GrammarBuilder(numChoices, 0, 9));
            NumberedChoices.Add(gb);
        }

        void LoadCommandsGrammar()
        {
            ChoicesPool.Add(AllCommands.ToArray());
            var gb1 = new GrammarBuilder(ChoicesPool);
            var gb2 = new GrammarBuilder(NumberedChoices);
            var choices = new Choices(new GrammarBuilder[] { gb1, gb2 });
            var gb = new GrammarBuilder(choices, 1, 10);
            CommandsGrammar = new Grammar(gb);
            Engine.LoadGrammar(CommandsGrammar);
        }

        void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result == null || e.Result.Confidence <= .7) return;
            List<string> splitwords = e.Result.Text.Split(' ').ToList();
            if (splitwords[0] == CurrentSettings.EnableKeyword || splitwords[0] == CurrentSettings.DisableKeyword)
            {
                ReloadGrammars(splitwords);
            }
            Console.Out.WriteLineAsync(e.Result.Text);
        }

        public void ReloadGrammars(List<string> splitwords)
        {
            string newVal = String.Join(" ", splitwords.Skip(2).Take(splitwords.Count - 2).ToArray());
            CurrentSettings.Scopes[splitwords[1]].Current = newVal;
            Engine.UnloadGrammar(CommandsGrammar);
            LoadCommands();
            LoadCommandsGrammar();
        }

        public void ListenIO()
        {
            while (true) Thread.Sleep(3000);
        }

    }
}
