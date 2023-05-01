using TrackerLibrary;
using TrackerLibrary.Models;

namespace TrackerUI
{
    public partial class CreateTournamentForm : Form, IPrizeRequester, ITeamRequester
    {
        List<TeamModel> avaliableTeams = GlobalConfig.Connection.GetTeam_All();
        List<TeamModel> selectedTeams = new List<TeamModel>();
        List<PrizeModel> selectedPrizes = new List<PrizeModel>();

        public CreateTournamentForm()
        {
            InitializeComponent();

            WireUpLists();
        }

        private void WireUpLists()
        {
            selectTeamDropDown.DataSource = null;

            selectTeamDropDown.DataSource = avaliableTeams;
            selectTeamDropDown.DisplayMember = "TeamName";

            tournamentTeamsListBox.DataSource = null;

            tournamentTeamsListBox.DataSource = selectedTeams;
            tournamentTeamsListBox.DisplayMember = "TeamName";

            prizesListBox.DataSource = null;

            prizesListBox.DataSource = selectedPrizes;
            prizesListBox.DisplayMember = "PlaceName";
        }

        private void addTeamButton_Click(object sender, EventArgs e)
        {
            TeamModel model = (TeamModel)selectTeamDropDown.SelectedItem;

            if (model != null)
            {
                avaliableTeams.Remove(model);
                selectedTeams.Add(model);

                WireUpLists();
            }
        }

        private void removeSelectedPlayerButton_Click(object sender, EventArgs e)
        {
            TeamModel model = (TeamModel)tournamentTeamsListBox.SelectedItem;

            if (model != null)
            {
                selectedTeams.Remove(model);
                avaliableTeams.Add(model);

                WireUpLists();
            }
        }

        private void removeSelectedPrizeButton_Click(object sender, EventArgs e)
        {
            PrizeModel model = (PrizeModel)prizesListBox.SelectedItem;

            if (model != null)
            {
                selectedPrizes.Remove(model);

                WireUpLists();
            }

        }

        private void createPrizeButton_Click(object sender, EventArgs e)
        {
            CreatePrizeForm frm = new CreatePrizeForm(this);
            frm.Show();
        }

        private void createNewTeamLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            CreateTeamForm frm = new CreateTeamForm(this);
            frm.Show();
        }

        public void PrizeComplite(PrizeModel model)
        {
            selectedPrizes.Add(model);
            WireUpLists();
        }

        public void TeamComplete(TeamModel model)
        {
            selectedTeams.Add(model);
            WireUpLists();
        }

        private void createTournamentButton_Click(object sender, EventArgs e)
        {
            // Validate data
            decimal fee = 0;

            if(!decimal.TryParse(entryFeeValue.Text, out fee))
            {
                MessageBox.Show("You need to enter a valid Entry Fee.", "Invalid Fee", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Create TournamentModel
            TournamentModel tournament = new TournamentModel();

            tournament.TournamentName = tournamentNameValue.Text;
            tournament.EntryFee = fee;
            tournament.Prizes = selectedPrizes;
            tournament.EnteredTeams = selectedTeams;

            TournamentLogic.CreateRounds(tournament);

            GlobalConfig.Connection.CreateTournament(tournament);

            // TODO: Napraw by dzialalo
            //tournament.AlertUsersToNewRound();

            TournamnetViewerForm frm = new TournamnetViewerForm(tournament);
            frm.Show();
            this.Close();
        }
    }
}