using TrackerLibrary;
using TrackerLibrary.Models;

namespace TrackerUI
{
    public partial class TournamnetViewerForm : Form
    {
        private TournamentModel tournament;
        List<int> rounds = new List<int>();
        List<MatchupModel> selectedMatchups = new List<MatchupModel>();

        public TournamnetViewerForm(TournamentModel tournamentModel)
        {
            InitializeComponent();

            tournament = tournamentModel;

            LoadFormData();

            LoadRounds();
        }

        private void LoadFormData()
        {
            tournamentName.Text = tournament.TournamentName;
        }

        private void WireUpRoundsLists()
        {
            roundDropDown.DataSource = null;
            roundDropDown.DataSource = rounds;
        }

        private void WireUpMatchupsLists()
        {
            matchupListBox.DataSource = null;
            matchupListBox.DataSource = selectedMatchups;
            matchupListBox.DisplayMember = "DisplayName";
        }

        private void LoadRounds()
        {
            int currentRound = 0;
            rounds = new List<int>();

            foreach(List<MatchupModel> matchups in tournament.Rounds)
            {
                if(matchups.First().MatchupRound > currentRound)
                {
                    currentRound++;
                    rounds.Add(currentRound);
                }
            }

            WireUpRoundsLists();
        }

        private void roundDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMatchups();
        }

        private void LoadMatchups()
        {
            selectedMatchups = new List<MatchupModel>();
            int round = (int)roundDropDown.SelectedItem;

            foreach (List<MatchupModel> matchups in tournament.Rounds)
            {
                if (matchups.First().MatchupRound == round)
                {
                    foreach(MatchupModel matchup in matchups)
                    {
                        if(matchup.Winner == null || !unplayedOnlyCheckBox.Checked)
                        {
                            selectedMatchups.Add(matchup);
                        }
                    }
                    break;
                }
            }

            WireUpMatchupsLists();
        }

        private void LoadMatchup()
        {
            MatchupModel matchup = (MatchupModel)matchupListBox.SelectedItem;

            bool visable = matchup != null;

            teamOneName.Visible = visable;
            teamOneScoreLabel.Visible = visable;
            teamOneScoreValue.Visible = visable;
            teamTwoName.Visible = visable;
            teamTwoScoreLabel.Visible = visable;
            teamTwoScoreValue.Visible = visable;
            versusLabel.Visible = visable;
            scoreButton.Visible = visable;

            if (matchup == null)
            {
                return;
            }

            if (matchup.Entries[0].TeamCompeting == null)
            {
                teamOneName.Text = "Not Yet Set";
                teamOneScoreValue.Text = "";
            }
            else
            {
                teamOneName.Text = matchup.Entries[0].TeamCompeting.TeamName;
                teamOneScoreValue.Text = matchup.Entries[0].Score.ToString();
            }

            if (matchup.Entries.Count > 1)
            {
                if(matchup.Entries[1].TeamCompeting != null)
                {
                    teamTwoName.Text = matchup.Entries[1].TeamCompeting.TeamName;
                    teamTwoScoreValue.Text = matchup.Entries[1].Score.ToString();
                }
                else
                {
                    teamTwoName.Text = "Not Yet Set";
                    teamTwoScoreValue.Text = "";
                }
                
            }
            else
            {
                teamTwoName.Text = "<bye>";
                teamTwoScoreValue.Text = "0";
            }
        }

        private void matchupListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMatchup();
        }

        private void unplayedOnlyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            LoadMatchups();
        }

        private bool IsValidData()
        {
            if(!double.TryParse(teamOneScoreValue.Text, out double teamOneScore) || !double.TryParse(teamTwoScoreValue.Text, out double teamTwoScore))
            {
                return false;
            }

            if(teamOneScore == teamTwoScore)
            {
                return false;
            }

            return true;
        }

        private void scoreButton_Click(object sender, EventArgs e)
        {
            if(!IsValidData())
            {
                MessageBox.Show("You need to enter valid data before we can score this matchup.");
                return;
            }

            MatchupModel matchup = (MatchupModel)matchupListBox.SelectedItem;
            double teamOneScore;
            double teamTwoScore = 0;

            if (matchup == null || matchup.Entries[0].TeamCompeting == null)
            {
                MessageBox.Show("You can't add score to this matchup yet.");
                return;
            }

            if(double.TryParse(teamOneScoreValue.Text, out teamOneScore))
            {
                matchup.Entries[0].Score = teamOneScore;
            }
            else
            {
                MessageBox.Show("Please enter a valid score for team 1.");
                return;
            }

            if (matchup.Entries.Count > 1)
            {
                if (double.TryParse(teamTwoScoreValue.Text, out teamTwoScore))
                {
                    matchup.Entries[1].Score = teamTwoScore;
                }
                else
                {
                    MessageBox.Show("Please enter a valid score for team 2.");
                    return;
                }
            }

            try
            {
                TournamentLogic.UpdateTournamentResults(tournament);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"The application had the fallowing error: {ex.Message}");
            }

            LoadMatchups();
        }
    }
}