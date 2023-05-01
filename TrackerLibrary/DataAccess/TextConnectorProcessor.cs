using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TrackerLibrary.DataAccess.TextHelpers
{
    internal static class TextConnectorProcessor
    {
        public static string FullFilePath(this string fileName)
        {
            return $"{ConfigurationManager.AppSettings["filePath"]}//{fileName}";
        }

        public static List<string> LoadFile(this string file)
        {
            if(!File.Exists(file))
            {
                return new List<string>();
            }

            return File.ReadAllLines(file).ToList();
        }

        public static List<PrizeModel> ConvertToPrizeModels(this List<string> lines)
        {
            List<PrizeModel> output = new List<PrizeModel>();

            foreach(string line in lines)
            {
                string[] cols = line.Split(',');

                PrizeModel prizeModel = new PrizeModel(cols[2], cols[1], cols[3], cols[4]);
                prizeModel.Id = int.Parse(cols[0]);

                output.Add(prizeModel);
            }

            return output;
        }

        public static List<PersonModel> ConvertToPersonModels(this List<string> lines)
        {
            List<PersonModel> output = new List<PersonModel>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                PersonModel personModel = new PersonModel();
                personModel.Id = int.Parse(cols[0]);
                personModel.FirstName = cols[1];
                personModel.LastName = cols[2];
                personModel.EmailAddress = cols[3];
                personModel.CellphoneNumber = cols[4];

                output.Add(personModel);
            }

            return output;
        }

        public static List<TeamModel> ConvertToTeamModels(this List<string> lines)
        {
            List<TeamModel> output = new List<TeamModel>();
            List<PersonModel> people = GlobalConfig.PeopleFile.FullFilePath().LoadFile().ConvertToPersonModels();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                TeamModel teamModel = new TeamModel();
                teamModel.Id = int.Parse(cols[0]);
                teamModel.TeamName = cols[1];

                string[] personIds = cols[2].Split('|');

                foreach(string personId in personIds)
                {
                    teamModel.TeamMembers.Add(people.Where(x => x.Id == int.Parse(personId)).First());
                }

                output.Add(teamModel);
            }

            return output;
        }

        public static List<TournamentModel> ConvertToTournamentModels(this List<string> lines)
        {
            List<TournamentModel> output = new List<TournamentModel>();
            List<TeamModel> teams = GlobalConfig.TeamsFile.FullFilePath().LoadFile().ConvertToTeamModels();
            List<PrizeModel> prizes = GlobalConfig.PrizesFile.FullFilePath().LoadFile().ConvertToPrizeModels();
            List<MatchupModel> matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile().ConvertToMatchupModels();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                TournamentModel tournamentModel = new TournamentModel();
                tournamentModel.Id = int.Parse(cols[0]);
                tournamentModel.TournamentName = cols[1];
                tournamentModel.EntryFee = decimal.Parse(cols[2]);

                string[] teamIds = cols[3].Split('|');

                foreach (string teamId in teamIds)
                {
                    tournamentModel.EnteredTeams.Add(teams.Where(x => x.Id == int.Parse(teamId)).First());
                }

                if (cols[4].Length > 0)
                {
                    string[] prizeIds = cols[4].Split('|');

                    foreach (string prizeId in prizeIds)
                    {
                        tournamentModel.Prizes.Add(prizes.Where(x => x.Id == int.Parse(prizeId)).First());
                    } 
                }

                // Capture Rounds information
                string[] roundIds = cols[5].Split('|');

                foreach (string roundId in roundIds)
                {
                    string[] matchupsIds = roundId.Split('^');
                    List<MatchupModel> round = new List<MatchupModel>();

                    foreach (string matchupsId in matchupsIds)
                    {
                        round.Add(matchups.Where(x => x.Id == int.Parse(matchupsId)).First());
                    }

                    tournamentModel.Rounds.Add(round);
                }

                output.Add(tournamentModel);
            }

            return output;
        }

        public static List<MatchupModel> ConvertToMatchupModels(this List<string> lines)
        {
            List<MatchupModel> output = new List<MatchupModel>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                MatchupModel matchupModel = new MatchupModel();
                matchupModel.Id = int.Parse(cols[0]);
                matchupModel.Entries = ConvertStringToMatchupEntryModels(cols[1]);
                if(cols[2].Length == 0)
                {
                    matchupModel.Winner = null;
                }
                else
                {
                    matchupModel.Winner = LookupTeamById(int.Parse(cols[2]));
                }
                matchupModel.MatchupRound = int.Parse(cols[3]);

                output.Add(matchupModel);
            }

            return output;
        }

        public static List<MatchupEntryModel> ConvertToMatchupEntryModels(this List<string> lines)
        {
            List<MatchupEntryModel> output = new List<MatchupEntryModel>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');
                int parentId = 0;

                MatchupEntryModel matchupEntryModel = new MatchupEntryModel();
                matchupEntryModel.Id = int.Parse(cols[0]);
                if(cols[1].Length == 0)
                {
                    matchupEntryModel.TeamCompeting = null;
                }
                else
                {
                    matchupEntryModel.TeamCompeting = LookupTeamById(int.Parse(cols[1]));
                }
                matchupEntryModel.Score = double.Parse(cols[2]);
                if (int.TryParse(cols[3], out parentId))
                {
                    matchupEntryModel.ParentMatchup = LookupMatchupById(parentId);
                }
                else
                {
                    matchupEntryModel.ParentMatchup = null;
                }

                output.Add(matchupEntryModel);
            }

            return output;
        }

        private static List<MatchupEntryModel> ConvertStringToMatchupEntryModels(string input)
        {
            string[] ids = input.Split('^');
            List<MatchupEntryModel> output = new List<MatchupEntryModel>();
            List<string> entries = GlobalConfig.MatchupEntryFile.FullFilePath().LoadFile();
            List<string> matchingEntries = new List<string>();

            foreach (string id in ids)
            {
                foreach(string entry in entries)
                {
                    string[] cols = entry.Split(',');

                    if (cols[0] == id)
                    {
                        matchingEntries.Add(entry);
                    }
                }
            }

            output = matchingEntries.ConvertToMatchupEntryModels();

            return output;
        }

        private static TeamModel LookupTeamById(int id)
        {
            List<string> teams = GlobalConfig.TeamsFile.FullFilePath().LoadFile();
            List<string> matchingTeams = new List<string>();

            foreach (string team in teams)
            {
                string[] cols = team.Split(',');

                if (cols[0] == id.ToString())
                {
                    matchingTeams.Add(team);
                    return matchingTeams.ConvertToTeamModels().First();
                }
            }

            return null;
        }

        private static MatchupModel LookupMatchupById(int id)
        {
            List<string> matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile();
            List<string> matchingMatchups = new List<string>();

            foreach (string matchup in matchups)
            {
                string[] cols = matchup.Split(',');

                if (cols[0] == id.ToString())
                {
                    matchingMatchups.Add(matchup);
                    return matchingMatchups.ConvertToMatchupModels().First();
                }
            }

            return null;
        }

        public static void SaveToPrizeFile(this List<PrizeModel> models)
        {
            List<string> lines = new List<string>();

            foreach (PrizeModel model in models)
            {
                lines.Add($"{model.Id},{model.PlaceNumber},{model.PlaceName},{model.PrizeAmount},{model.PrizeProcentage}");
            }

            File.WriteAllLines(GlobalConfig.PrizesFile.FullFilePath(), lines);
        }

        public static void SaveToPeopleFile(this List<PersonModel> models)
        {
            List<string> lines = new List<string>();

            foreach (PersonModel model in models)
            {
                lines.Add($"{model.Id},{model.FirstName},{model.LastName},{model.EmailAddress},{model.CellphoneNumber}");
            }

            File.WriteAllLines(GlobalConfig.PeopleFile.FullFilePath(), lines);
        }

        public static void SaveToTeamsFile(this List<TeamModel> models)
        {
            List<string> lines = new List<string>();

            foreach (TeamModel model in models)
            {
                lines.Add($"{model.Id},{model.TeamName},{model.TeamMembers.ConvertPeopleListToString()}");
            }

            File.WriteAllLines(GlobalConfig.TeamsFile.FullFilePath(), lines);
        }

        public static void SaveToTournamentsFile(this List<TournamentModel> models)
        {
            List<string> lines = new List<string>();

            foreach (TournamentModel model in models)
            {
                lines.Add($"{model.Id},{model.TournamentName},{model.EntryFee},{model.EnteredTeams.ConvertTeamListToString()},{model.Prizes.ConvertPrizeListToString()},{model.Rounds.ConvertRoundListToString()}");
            }

            File.WriteAllLines(GlobalConfig.TournamentsFile.FullFilePath(), lines);
        }

        public static void SaveRoundsToFile(this TournamentModel model)
        {
            foreach(List<MatchupModel> round in model.Rounds)
            {
                foreach(MatchupModel matchup in round)
                {
                    matchup.SaveMatchupToFile();
                }
            }
        }

        private static void SaveMatchupToFile(this MatchupModel model)
        {
            List<MatchupModel> matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile().ConvertToMatchupModels();

            int currentId = 0;

            if(matchups.Count > 0)
            {
                currentId = matchups.OrderBy(x => x.Id).Last().Id;
            }

            model.Id = currentId + 1;
            matchups.Add(model);

            foreach (MatchupEntryModel entry in model.Entries)
            {
                entry.SaveEntryToFile();
            }

            List<string> lines = new List<string>();

            foreach (MatchupModel matchup in matchups)
            {
                string winner = "";
                if (matchup.Winner != null)
                {
                    winner = matchup.Winner.Id.ToString();
                }
                lines.Add($"{matchup.Id},{matchup.Entries.ConvertMatchupEntryListToString()},{winner},{matchup.MatchupRound}");
            }

            File.WriteAllLines(GlobalConfig.MatchupFile.FullFilePath(), lines);
        }

        public static void UpdateMatchupToFile(this MatchupModel model)
        {
            List<MatchupModel> matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile().ConvertToMatchupModels();
            MatchupModel oldMatchup = new MatchupModel();

            foreach(MatchupModel matchup in matchups)
            {
                if(matchup.Id == model.Id)
                {
                    oldMatchup = matchup;
                    break;
                }
            }

            matchups.Remove(oldMatchup);
            matchups.Add(model);

            foreach (MatchupEntryModel entry in model.Entries)
            {
                entry.UpdateEntryToFile();
            }

            List<string> lines = new List<string>();

            foreach (MatchupModel matchup in matchups)
            {
                string winner = "";
                if (matchup.Winner != null)
                {
                    winner = matchup.Winner.Id.ToString();
                }
                lines.Add($"{matchup.Id},{matchup.Entries.ConvertMatchupEntryListToString()},{winner},{matchup.MatchupRound}");
            }

            File.WriteAllLines(GlobalConfig.MatchupFile.FullFilePath(), lines);
        }

        private static void UpdateEntryToFile(this MatchupEntryModel model)
        {
            List<MatchupEntryModel> entries = GlobalConfig.MatchupEntryFile.FullFilePath().LoadFile().ConvertToMatchupEntryModels();
            MatchupEntryModel oldMatchupEntry = new MatchupEntryModel();

            foreach (MatchupEntryModel entry in entries)
            {
                if (entry.Id == model.Id)
                {
                    oldMatchupEntry = entry;
                    break;
                }
            }

            entries.Remove(oldMatchupEntry);
            entries.Add(model);

            List<string> lines = new List<string>();

            foreach (MatchupEntryModel entrie in entries)
            {
                string parent = "";
                if(entrie.ParentMatchup != null)
                {
                    parent = entrie.ParentMatchup.Id.ToString();
                }
                string teamCompeting = "";
                if (entrie.TeamCompeting != null)
                {
                    teamCompeting = entrie.TeamCompeting.Id.ToString();
                }
                lines.Add($"{entrie.Id},{teamCompeting},{entrie.Score},{parent}");
            }

            File.WriteAllLines(GlobalConfig.MatchupEntryFile.FullFilePath(), lines);
        }

        private static void SaveEntryToFile(this MatchupEntryModel model)
        {
            List<MatchupEntryModel> entries = GlobalConfig.MatchupEntryFile.FullFilePath().LoadFile().ConvertToMatchupEntryModels();

            int currentId = 0;

            if (entries.Count > 0)
            {
                currentId = entries.OrderBy(x => x.Id).Last().Id;
            }

            model.Id = currentId + 1;
            entries.Add(model);

            List<string> lines = new List<string>();

            foreach (MatchupEntryModel entrie in entries)
            {
                string parent = "";
                if (entrie.ParentMatchup != null)
                {
                    parent = entrie.ParentMatchup.Id.ToString();
                }
                string teamCompeting = "";
                if (entrie.TeamCompeting != null)
                {
                    teamCompeting = entrie.TeamCompeting.Id.ToString();
                }
                lines.Add($"{entrie.Id},{teamCompeting},{entrie.Score},{parent}");
            }

            File.WriteAllLines(GlobalConfig.MatchupEntryFile.FullFilePath(), lines);
        }

        private static string ConvertPeopleListToString(this List<PersonModel> models)
        {
            string output = "";

            if(models.Count == 0)
            {
                return output;
            }

            foreach(PersonModel model in models)
            {
                output += $"{model.Id}|";
            }

            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertTeamListToString(this List<TeamModel> models)
        {
            string output = "";

            if (models.Count == 0)
            {
                return output;
            }

            foreach (TeamModel model in models)
            {
                output += $"{model.Id}|";
            }

            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertPrizeListToString(this List<PrizeModel> models)
        {
            string output = "";

            if (models.Count == 0)
            {
                return output;
            }

            foreach (PrizeModel model in models)
            {
                output += $"{model.Id}|";
            }

            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertRoundListToString(this List<List<MatchupModel>> models)
        {
            string output = "";

            if (models.Count == 0)
            {
                return output;
            }

            foreach (List<MatchupModel> matchups in models)
            {
                output += $"{matchups.ConvertMatchupListToString()}|";
            }

            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertMatchupListToString(this List<MatchupModel> models)
        {
            string output = "";

            if (models.Count == 0)
            {
                return output;
            }

            foreach (MatchupModel model in models)
            {
                output += $"{model.Id}^";
            }

            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertMatchupEntryListToString(this List<MatchupEntryModel> models)
        {
            string output = "";

            if (models.Count == 0)
            {
                return output;
            }

            foreach (MatchupEntryModel model in models)
            {
                output += $"{model.Id}^";
            }

            output = output.Substring(0, output.Length - 1);

            return output;
        }
    }
}
