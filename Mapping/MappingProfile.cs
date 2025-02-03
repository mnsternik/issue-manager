using AutoMapper;
using IssueManager.Models;
using IssueManager.Models.ViewModels.Categories;
using IssueManager.Models.ViewModels.Requests;
using IssueManager.Models.ViewModels.Teams;
using IssueManager.Models.ViewModels.UserManagement;

namespace IssueManager.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {

            CreateMap<CreateRequestViewModel, Request>();
            CreateMap<EditRequestViewModel, Request>(); 
            CreateMap<Request, DetailsRequestViewModel>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.UserName))
                .ForMember(dest => dest.AssignedUserName, opt => opt.MapFrom(src => src.AssignedUser.UserName));
            CreateMap<Request, EditRequestViewModel>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.UserName));
            CreateMap<Request, RequestsListItemViewModel>()
                .ForMember(dest => dest.AssignedUserName, opt => opt.MapFrom(src => src.AssignedUser.UserName));
            CreateMap<RequestResponse, RequestResponseViewModel>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.UserName));

            CreateMap<User, UsersListItemViewModel>();
            CreateMap<ManageUserViewModel, User>();
            CreateMap<User, ManageUserViewModel>();

            CreateMap<Team, TeamsListItemViewModel>();
            CreateMap<CreateTeamViewModel, Team>();

            CreateMap<Category, CategoriesListItemViewModel>();
            CreateMap<CreateCategoryViewModel, Category>();
        }
    }
}
