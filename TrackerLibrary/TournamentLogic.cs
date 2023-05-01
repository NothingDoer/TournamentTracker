using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TrackerLibrary
{
    public static class TournamentLogic
    {
        public static void CreateRounds(TournamentModel model)
        {
            List<TeamModel> randomizedTeams = RanodmizeTeamsOrder(model.EnteredTeams);
            int rounds = FindNumberOfRounds(randomizedTeams.Count);
            int byes = NumberOfByes(rounds, randomizedTeams.Count);

            model.Rounds.Add(CreateFirstRound(randomizedTeams, byes));

            CreateOtherRounds(model, rounds);
        }

        public static void UpdateTournamentResults(TournamentModel model)
        {
            int startingRound = model.CheckCurrentRound();
            List<MatchupModel> toScore = new List<MatchupModel>();

            foreach (List<MatchupModel> round in model.Rounds)
            {
                foreach (MatchupModel matchup in round)
                {
                    if (matchup.Entries.Any(x => x.Score != 0) || matchup.Entries.Count == 1)
                    {
                        toScore.Add(matchup);
                    }
                }
            }

            MarkWinnerInMatchups(toScore);

            AdvanceWinners(toScore, model);

            toScore.ForEach(x => GlobalConfig.Connection.UpdateMatchup(x));
            int endingRound = model.CheckCurrentRound();

            if(endingRound > startingRound)
            {
                // TODO: Naprawic
                //model.AlertUsersToNewRound();
            }
        }

        public static void AlertUsersToNewRound(this TournamentModel model)
        {
            int currentRoundNumber = model.CheckCurrentRound();
            List<MatchupModel> currentRound = model.Rounds.Where(x => x.First().MatchupRound == currentRoundNumber).First();

            foreach(MatchupModel matchup in currentRound)
            {
                foreach (MatchupEntryModel entry in matchup.Entries)
                {
                    foreach(PersonModel person in entry.TeamCompeting.TeamMembers)
                    {
                        AlertPersonToNewRound(person, entry.TeamCompeting.TeamName, matchup.Entries.Where(x => x.TeamCompeting != entry.TeamCompeting).FirstOrDefault());
                    }
                }
            }
        }

        private static void AlertPersonToNewRound(PersonModel person, string teamName, MatchupEntryModel? competitor)
        {
            if(person.EmailAddress.Length == 0)
            {
                return;
            }

            string to = "";
            string subject = "";
            StringBuilder body = new StringBuilder();

            if(competitor != null)
            {
                subject = $"You have a new matchup with {competitor.TeamCompeting.TeamName}";

                body.AppendLine("<h1>You have a new matchup</h1>");
                body.Append("<strong>Competitor: </strong>");
                body.AppendLine(competitor.TeamCompeting.TeamName);
                body.AppendLine();
                body.AppendLine("Have a great time!");
                body.AppendLine("~Tournament Tracker");
            }
            else
            {
                subject = "You have a bye week this round";

                body.AppendLine("Enjoy your round off!");
                body.AppendLine("~Tournament Tracker");
            }

            to = person.EmailAddress;

            EmailLogic.SendEmail(to, subject, body.ToString());
        }

        private static int CheckCurrentRound(this TournamentModel model)
        {
            int output = 1;

            foreach(List<MatchupModel> round in model.Rounds)
            {
                if(round.All(x => x.Winner != null))
                {
                    output++;
                }
            }

            return output;
        }

        private static void MarkWinnerInMatchups(List<MatchupModel> models)
        {
            foreach (MatchupModel model in models)
            {
                if (model.Entries.Count == 1)
                {
                    model.Winner = model.Entries[0].TeamCompeting;
                    continue;
                }

                if (model.Entries[0].Score > model.Entries[1].Score)
                {
                    model.Winner = model.Entries[0].TeamCompeting;
                }
                else if (model.Entries[1].Score > model.Entries[0].Score)
                {
                    model.Winner = model.Entries[1].TeamCompeting;
                }
                else
                {
                    throw new Exception("We do not allow ties in this application.");
                }
            }
        }

        private static void AdvanceWinners(List<MatchupModel> models, TournamentModel tournament)
        {
            foreach (MatchupModel matchup in models)
            {
                foreach (List<MatchupModel> round in tournament.Rounds)
                {
                    foreach (MatchupModel matchupModel in round)
                    {
                        foreach (MatchupEntryModel matchupEntry in matchupModel.Entries)
                        {
                            if (matchupEntry.ParentMatchup != null && matchupEntry.ParentMatchup.Id == matchup.Id)
                            {
                                matchupEntry.TeamCompeting = matchup.Winner;
                                GlobalConfig.Connection.UpdateMatchup(matchupModel);
                            }
                        }
                    }
                } 
            }
        }

        private static void CreateOtherRounds(TournamentModel model, int rounds)
        {
            int round = 2;
            List<MatchupModel> previousRound = model.Rounds[0];
            List<MatchupModel> currentRound = new List<MatchupModel>();
            MatchupModel currentMatchup = new MatchupModel();

            while (round <= rounds)
            {
                foreach(MatchupModel match in previousRound)
                {
                    currentMatchup.Entries.Add(new MatchupEntryModel { ParentMatchup = match });

                    if(currentMatchup.Entries.Count > 1)
                    {
                        currentMatchup.MatchupRound = round;
                        currentRound.Add(currentMatchup);
                        currentMatchup = new MatchupModel();
                    }
                }

                model.Rounds.Add(currentRound);

                previousRound = currentRound;
                currentRound = new List<MatchupModel>();

                round++;
            }
        }

        private static List<MatchupModel> CreateFirstRound(List<TeamModel> teams, int byes)
        {
            List<MatchupModel> output = new List<MatchupModel>();
            MatchupModel currentModel = new MatchupModel();

            foreach (TeamModel team in teams)
            {
                currentModel.Entries.Add(new MatchupEntryModel { TeamCompeting = team });

                if (byes > 0 || currentModel.Entries.Count > 1)
                {
                    currentModel.MatchupRound = 1;
                    output.Add(currentModel);
                    currentModel = new MatchupModel();

                    if(byes > 0)
                    {
                        byes--;
                    }
                }
            }

            return output;
        }

        private static int NumberOfByes(int rounds, int numberOfTeams)
        {
            return (int)Math.Pow(2, rounds) - numberOfTeams;
        }

        private static int FindNumberOfRounds(int numberOfTeams)
        {
            return (int)Math.Ceiling(Math.Log2(numberOfTeams));
        }

        private static List<TeamModel> RanodmizeTeamsOrder(List<TeamModel> teams)
        {
            return teams.OrderBy(x => Guid.NewGuid()).ToList();
        }
    }
}
