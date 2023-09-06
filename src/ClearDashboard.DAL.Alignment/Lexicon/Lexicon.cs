using Microsoft.SqlServer.Server;
using Paratext.PluginInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.DAL.Alignment.Lexicon
{
    public class Lexicon
    {
#if DEBUG
        private ObservableCollection<Lexeme> lexemes_;
#else
        // RELEASE MODIFIED
        //private readonly ObservableCollection<Lexeme> lexemes_;
        private ObservableCollection<Lexeme> lexemes_;
#endif

        public ObservableCollection<Lexeme> Lexemes
        {
            get { return lexemes_; }
#if DEBUG
            set { lexemes_ = value; }
#else
            // RELEASE MODIFIED
            //set { lexemes_ = value; }
            set { lexemes_ = value; }
#endif
        }

        public Lexicon()
        {
            lexemes_ = new ObservableCollection<Lexeme>();
        }
        internal Lexicon(ICollection<Lexeme> lexemes)
        {
            lexemes_ = new ObservableCollection<Lexeme>(lexemes.DistinctBy(e => e.LexemeId));
        }
    }
}
