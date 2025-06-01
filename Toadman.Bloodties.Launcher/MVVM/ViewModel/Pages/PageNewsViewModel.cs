using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Toadman.GameLauncher.Core;

namespace Toadman.Bloodties.Launcher
{
    public class PageNewsViewModel : PageBaseViewModel
    {
        public ICommand NextCommand { get; set; }
        public ICommand PreviewCommand { get; set; }


        public List<NewsMenuItemViewModel> NewsMenu
        {
            get { return newsMenu; }
            set { SetField(ref newsMenu, value); }
        }

        public NewsItem SelectedNews
        {
            get { return selectedNews; }
            set { SetField(ref selectedNews, value); }
        }
        
        private List<NewsMenuItemViewModel> newsMenu;
        private NewsItem selectedNews;
        private int selectedItemIndex = 0;

        public PageNewsViewModel()
        {
            var newsList = new List<NewsItem>
            {
                new NewsItem("Mutants",
                "The Ascended are experimenting on their own, creating abominations of nature. Calling themselves the Chosen of the Prophet, these mutants are wreaking havoc across City Seven. Proving a match for even the Reapers, they pose a great threat to the power balance of the City.",
                "/BloodtiesLauncher;component/Resources/NewsImages/Mutants_small.png"),
                new NewsItem("Rumors",
                "Whispers of dissent among the vampire elite have surfaced. Rumors speak of a coalition of rogue vampires, dissatisfied with the rule of the Nephilim. They wish to break from tradition, and establish an autonomous rule of City Seven. Should these rumors prove to be true, it is up the Reapers to take control of the City and punish those who dare go against their ancient masters.",
                "/BloodtiesLauncher;component/Resources/NewsImages/Rumors_small.png"),
                new NewsItem("Fallen angel",
                "Is it possible the Nephilim were not the only ones to escape judgement? The Prophet claims to be in contact with a fallen angel on Earth, who is aiding the Ascended in their cause. There is no evidence to support his claim, but the Grandmaster has deemed it worthy of further investigation.",
                "/BloodtiesLauncher;component/Resources/NewsImages/Fallen_angel_small.png")
            };

            NewsMenu = newsList.Select(x => new NewsMenuItemViewModel()).ToList();

            SelectedNews = newsList[selectedItemIndex];
            NewsMenu[selectedItemIndex].IsSelected = true;

            NextCommand = new RelayCommand(() =>
            {
                NewsMenu[selectedItemIndex].IsSelected = false;
                selectedItemIndex++;
                if (selectedItemIndex == newsList.Count)
                    selectedItemIndex = 0;

                SelectedNews = newsList[selectedItemIndex];
                NewsMenu[selectedItemIndex].IsSelected = true;
            });

            PreviewCommand = new RelayCommand(() =>
            {
                NewsMenu[selectedItemIndex].IsSelected = false;
                selectedItemIndex--;
                if (selectedItemIndex == -1)
                    selectedItemIndex = newsList.Count - 1;

                NewsMenu[selectedItemIndex].IsSelected = true;
                SelectedNews = newsList[selectedItemIndex];
            });

            var selectCommand = new RelayCommand<int>((index) => 
            {
                NewsMenu[selectedItemIndex].IsSelected = false;
                selectedItemIndex = index;
                NewsMenu[selectedItemIndex].IsSelected = true;
                SelectedNews = newsList[selectedItemIndex];
            });

            for (int i = 0; i < NewsMenu.Count; i++)
            {
                NewsMenu[i].SelectCommand = selectCommand;
                NewsMenu[i].Index = i;
            }
        }
    }
}