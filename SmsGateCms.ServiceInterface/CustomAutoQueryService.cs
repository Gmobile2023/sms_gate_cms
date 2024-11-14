// using ServiceStack;
// using SmsGateCms.ServiceModel;
//
// namespace SmsGateCms.ServiceInterface;
//
// public class CustomAutoQueryService : Service
// {
//     public IAutoQueryDb AutoQuery { get; set; }
//     
//     public object Any(QueryMessages request)
//     {
//         var userSession = base.SessionAs<AuthUserSession>();
//
//         // Kiểm tra xem người dùng có vai trò "partner" hay không
//         if (userSession.Roles.Contains("partner"))
//         {
//             // Lọc theo CreatedBy = UserId khi vai trò là "partner"
//             request.CreatedBy = userSession.UserAuthId;
//         }
//
//         // Thực hiện truy vấn AutoQuery với bộ lọc tùy chỉnh
//         var query = AutoQuery.CreateQuery(request, Request.GetRequestParams());
//         return AutoQuery.Execute(request, query);
//     }
// }