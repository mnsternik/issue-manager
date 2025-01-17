﻿using AutoMapper;
using IssueManager.Models;
using IssueManager.Models.ViewModels.Requests;
using IssueManager.Models.ViewModels.UserManagement;

namespace IssueManager.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<CreateRequestViewModel, Request>();
            CreateMap<EditRequestViewModel, Request>(); 
            CreateMap<Request, DetailsRequestViewModel>();
            CreateMap<Request, EditRequestViewModel>();
            CreateMap<Request, RequestsListItemViewModel>();
            //CreateMap<RequestResponseViewModel, RequestResponse>();
            CreateMap<RequestResponse, RequestResponseViewModel>();

            CreateMap<User, UsersListItemViewModel>();
            CreateMap<ManageUserViewModel, User>();
            CreateMap<User, ManageUserViewModel>();
        }
    }
}
