using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;
using TrackerLibrary.DataAccess.TextHelpers;

namespace TrackerLibrary.DataAccess
{
    public class TextConnector : IDataConnection
    {
        public void CreatePerson(PersonModel model)
        {
            // Load text file and convert the text to List<PersonModel>
            List<PersonModel> people = GlobalConfig.PeopleFile.FullFilePath().LoadFile().ConvertToPersonModels();

            // Find the max Id
            int currentId = 0;
            if (people.Count > 0)
            {
                currentId = people.OrderBy(x => x.Id).Last().Id;
            }
            model.Id = currentId + 1;

            // Add the new record with the new Id
            people.Add(model);

            //Save the List<PrizeModel> to text file
            people.SaveToPeopleFile();
        }

        // TODO - Make the CreatePrice method actually save to the file.
        public void CreatePrice(PrizeModel model)
        {
            // Load text file and convert the text to List<PrizeModel>
            List<PrizeModel> prizes = GlobalConfig.PrizesFile.FullFilePath().LoadFile().ConvertToPrizeModels();

            // Find the max Id
            int currentId = 0;
            if(prizes.Count > 0)
            {
                currentId = prizes.OrderBy(x => x.Id).Last().Id;
            }
            model.Id = currentId + 1;

            // Add the new record with the new Id
            prizes.Add(model);

            //Save the List<PrizeModel> to text file
            prizes.SaveToPrizeFile();
        }

        public void CreateTeam(TeamModel model)
        {
            // Load text file and convert the text to List<PersonModel>
            List<TeamModel> teams = GlobalConfig.TeamsFile.FullFilePath().LoadFile().ConvertToTeamModels();

            // Find the max Id
            int currentId = 0;
            if (teams.Count > 0)
            {
                currentId = teams.OrderBy(x => x.Id).Last().Id;
            }
            model.Id = currentId + 1;

            // Add the new record with the new Id
            teams.Add(model);

            //Save the List<PrizeModel> to text file
            teams.SaveToTeamsFile();
        }

        public void CreateTournament(TournamentModel model)
        {
            // Load text file and convert the text to List<PrizeModel>
            List<TournamentModel> tournaments = GlobalConfig.TournamentsFile.FullFilePath().LoadFile().ConvertToTournamentModels();

            // Find the max Id
            int currentId = 0;
            if (tournaments.Count > 0)
            {
                currentId = tournaments.OrderBy(x => x.Id).Last().Id;
            }

            model.Id = currentId + 1;

            tournaments.Add(model);

            model.SaveRoundsToFile();

            tournaments.SaveToTournamentsFile();

            TournamentLogic.UpdateTournamentResults(model);
        }

        public List<PersonModel> GetPerson_All()
        {
            return GlobalConfig.PeopleFile.FullFilePath().LoadFile().ConvertToPersonModels();
        }

        public List<TeamModel> GetTeam_All()
        {
            return GlobalConfig.TeamsFile.FullFilePath().LoadFile().ConvertToTeamModels();
        }

        public List<TournamentModel> GetTournament_All()
        {
            return GlobalConfig.TournamentsFile.FullFilePath().LoadFile().ConvertToTournamentModels();
        }

        public void UpdateMatchup(MatchupModel model)
        {
            model.UpdateMatchupToFile();
        }
    }
}
