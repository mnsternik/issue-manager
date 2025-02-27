using AutoMapper;
using IssueManager.Models;
using IssueManager.Models.ViewModels.Categories;
using IssueManager.Models.ViewModels.Requests;
using IssueManager.Models.ViewModels.Teams;
using IssueManager.Models.ViewModels.Users;

namespace IssueManager.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            // ViewModel to Request
            CreateMap<CreateRequestViewModel, Request>();
            CreateMap<EditRequestViewModel, Request>();
            // Request to ViewModel
            CreateMap<Request, DetailsRequestViewModel>();
            CreateMap<Request, EditRequestViewModel>();
            CreateMap<Request, RequestsListItemViewModel>();

            // Request Response to ViewModel
            CreateMap<RequestResponse, RequestResponseViewModel>();

            // User to ViewModel
            CreateMap<User, UsersListItemViewModel>();
            CreateMap<User, ChangeUserDetailsViewModel>();
            CreateMap<User, ChangeUserPasswordViewModel>();
            CreateMap<User, ChangeUserRolesViewModel>();

            // ViewModel to User
            CreateMap<ChangeUserDetailsViewModel, User>();
            CreateMap<CreateUserViewModel, User>();

            // Team to ViewModel
            CreateMap<Team, TeamsListItemViewModel>();
            // ViewModel to Team
            CreateMap<CreateTeamViewModel, Team>();

            // Category to ViewModel
            CreateMap<Category, CategoriesListItemViewModel>();
            // ViewModel to Category
            CreateMap<CreateCategoryViewModel, Category>();
        }
    }
}
