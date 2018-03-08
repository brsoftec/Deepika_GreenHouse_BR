using System.Net;
using System.Net.Http;
using System.Web.Http;
using GH.Core.BlueCode.BusinessLogic;
using GH.Core.BlueCode.Entity.Campaign;
using GH.Core.Services;
using MongoDB.Bson;
using GH.Core.ViewModels;
using System.Collections.Generic;
using GH.Core.BlueCode.Entity.PostHandShake;
using GH.Core.BlueCode.DataAccess;
using MongoDB.Driver;
using GH.Core.BlueCode.Entity.Request;

namespace GH.Web.Areas.User.Controllers
{
    
    [RoutePrefix("api/campaign")]
    public class CampaignApiController : ApiController
    {
        private ICampaignService _campaignService;
        private IAccountService _accountService;
    
        public CampaignApiController()
        {
            _accountService = new AccountService();
            _campaignService = new CampaignService();
        }

        [Route("campaigns")]
        [HttpGet]
        public IHttpActionResult ListCampaign(string type= null)
        {
            var rs = new List<CampaignDto>();
            var lstCampaign = _campaignService.GetListByType();
            foreach(var item in lstCampaign)
            {
                var camp = new CampaignDto();
                camp.Id = item.Id.ToString();
                camp.userId = item.userId;
                camp.campaign = item.campaign;
                if (camp != null)
                    rs.Add(camp);
            }
            return Ok(rs);
        }
        [Route("campaigns/request")]
        [HttpGet]
        public IHttpActionResult ListCampaignForRequest(string type = null, string userId = null)
        {
            var rs = new List<CampaignDto>();
            var lstCampaign = _campaignService.GetCampaignActive(userId, type);
            var postHandShakeCollection = MongoDBConnection.Database.GetCollection<BsonDocument>("PostHandShake");
         
            foreach (var item in lstCampaign)
            {
                var camp = new CampaignDto();
                camp.Id = item.Id.ToString();
                var filter = Builders<BsonDocument>.Filter.Eq("campaignid", camp.Id);
                filter = filter & Builders<BsonDocument>.Filter.Eq("userId", userId);
                if (postHandShakeCollection.Find(filter).Count() > 0)
                    camp.status = EnumRequest.StatusComplete;
             

                camp.userId = item.userId;
                camp.campaign = item.campaign;
                if (camp != null)
                    rs.Add(camp);
            }
            return Ok(rs);
        }

        // [Route("campaign")]
        [Route("")]
        [HttpGet]
        public IHttpActionResult GetCampaignById(string id)
        {
            var rs = new CampaignDto();
           var camp = _campaignService.GetCampaignById(id);
            if (camp == null)
                return NotFound();
                
            rs.Id = camp.Id.ToString();
            rs.userId = camp.userId;
            rs.campaign = camp.campaign;
            
            return Ok(rs);
        }

        [Route("")]
        [HttpPost]
        public IHttpActionResult InsertCampaign(CampaignDto campaign)
        {
            if (campaign == null)
                return BadRequest();
            var camp = new Campaign();
            if(!string.IsNullOrEmpty(campaign.Id))
                camp.Id = new MongoDB.Bson.ObjectId(campaign.Id);
            else
             camp.Id = ObjectId.GenerateNewId();
            camp.userId = campaign.userId;
            camp.campaign = campaign.campaign;
           
            var rs = _campaignService.InsertCampaign(camp);
            return Ok(rs);
        }
        [Route("")]
        [HttpPut]
        public IHttpActionResult UpdateCampaign(CampaignDto campaign)
        {
            if (campaign == null)
                return BadRequest();
            var camp = new Campaign();
            camp.Id = new MongoDB.Bson.ObjectId(campaign.Id);
            camp.userId = campaign.userId;
            camp.campaign = campaign.campaign;

            var rs = _campaignService.Update(camp);
            
            return Ok(rs);
        }
        [Route("")]
        [HttpDelete]
        public IHttpActionResult DeleteCampaign(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();
            var rs = _campaignService.DeleteCampaign(id);
            return Ok(rs);
        }

        //var handshake =
        //new PostHandShakeBusinessLogic().GetPostHandShakeByuserIdandCamapignid(postHandShakeModel.Userid,
        //    postHandShakeModel.CampaignId);

        [Route("handshakes")]
        [HttpGet]
        public IHttpActionResult GetListHandshake(string userId = null)
        {
            var postHandShake = new PostHandShakeBusinessLogic();
            var rs = new List<PostHandShake>();
            if (string.IsNullOrEmpty(userId))
                return BadRequest();
             rs = postHandShake.GetAllPostHandShakeByuserId(userId);
            return Ok(rs);
        }
    }
}
