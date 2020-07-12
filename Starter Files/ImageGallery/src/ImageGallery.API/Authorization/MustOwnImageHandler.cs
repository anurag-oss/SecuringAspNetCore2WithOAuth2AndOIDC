using System;
using System.Linq;
using System.Threading.Tasks;

using ImageGallery.API.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ImageGallery.API.Authorization
{
    public class MustOwnImageHandler : AuthorizationHandler<MustOwnImageRequirement>
    {
        private readonly IGalleryRepository galleryRepository;

        public MustOwnImageHandler(IGalleryRepository galleryRepository)
        {
            this.galleryRepository = galleryRepository;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MustOwnImageRequirement requirement)
        {
            var filterContext = context.Resource as AuthorizationFilterContext;
            if (filterContext == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var imageId = filterContext.RouteData.Values["id"].ToString();
            if (!Guid.TryParse(imageId, out Guid imageidAsGuid))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var ownerId = context.User.Claims.FirstOrDefault(c => c.Type == "sub").Value;
            if (!this.galleryRepository.IsImageOwner(imageidAsGuid, ownerId))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;

        }
    }
}
