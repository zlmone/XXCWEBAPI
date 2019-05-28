﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http;
using XXCWEBAPI.Models;
using XXCWEBAPI.Utils;

namespace XXCWEBAPI.Controllers
{   
    [RoutePrefix("api/visitorAccessInf")]
    public class VisitorAccessInfController : ApiController
    {
        private string privateKey = "MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCxpxhaLcSHVI+/zCNbqNpHw28GtLEPXBmzV3OVpmr65mg+KexOSmN7L0C+5Jnd0gTxJaVsp0MBOwtGmLHAHdzE+VFHXNGdFlA8B/z099n6lRx5UXSS0a5XwD2VQknZmQF15ek2WTw1qIHJ8GC4nBiJuEO0gJYCTiGQUE3jy5QTD0AUz1siZh9g/lZfcSn3safHzXHWnBqf9VVugSQiuN/ByDalurTFDr2CI04KI0yMNF1NvPI7iWcx8smvqVG/s9Ns7QhOkLeyelyunPX0jeZ1S3wQ5R3xqbt6W8c8mR0v2lk17fczCLjGDjAkBd8DGYLX56N8LZ2zEovLLeHqJA+PAgMBAAECggEABcCNT04wENmyFdm8Q1mCR9SSIbt0CDVJN79bJLtQt3MCaRDeb+KEuhZbmFK6kK4eLtizNINt7fpFcTG8f6X34gDYmuDsgJOaYXc4v43O5wgw9dSnW6GibYDx/YU58uu7Wl/pXzMgefRMz4cS+qdDPCJVPuDy+nwhJhUTkI6k6sED2E23MsIPFpA78k6TVpEGOS9RC1++A6mj7NOrgGhIO7BkgmVfRRpAkVKABGEaNHav8nTZ1IIQYCshRbyPAni/W/KmN2URSlED5tX4/VdJP/PyQE4BO6OuJ2wnkJkv2JMDNwicLZBVz+pS8PfBS1kHyrYsC3xv3ybUbIgBtnUH4QKBgQDYnwYXB314QUWr7VzkbH53vAi4JhC4AoycTkOYgerkjXt9R5cJPT2/Ofv2ZmxQuz6cQ6aTynxfdtN7Cb+VGZI1FGkklV1KzwhhmLjuaNhMRDE/XUDSXkRaj55cX8ik6pPMBOSNrt9T+6Y0H5kr3PcnSkCpfF1wO7PeUSCEDTQiyQKBgQDR8pT3zYwE6E7sQBIkl3YrMfJMyci+nXNjp+O/vFY6bHFf/5I3qHJzWthIF2UeXzVnJQPBccmbIEdtbXk1petZvOMtEVrptJmGUrV7r2LFeRn4oUYCgqMr8XVldqlUG24lrROkIHNNHWQFRXyXwEYrz4bLeR6r8pSI4QcQWwmzlwKBgCNAVbRfsqpkLNtaqDg/86C2h9C32Raoy4sQLW3fDoOdBpCPmuOVBLxeykMBzfShVAIH/E6mr/C1HJs0LeosnB9pL+cVK3ZmFJ4VRVr+0twuaLlACrFxR7xZDNNJfxRfXCfiT/NClvNKy3RGBB4gOlQ5gCZUp7wA6zdtilYS8/4JAoGAE7Rn5OYm2SMQnT3aNhL9JUq3yhs6OyG9/cF5L7q2gR9CeNcc2xp1O3xwRjvj4rje40JnGtXaLTQXYB7hPHbJIxAGZml1le+8ZQ4IOIaah5w5IsvILV4jgHFWKmK7u8gjS2f2KvZcvAUhKRl/eyKxs1Tz+s7wYQUQidRM/Gz++RsCgYEAh13t8/5+k3MhWRfuo9YmUcGAGyrH7HQwZdVXIWRfIq23l+suFGRLsrbYTPBDkgIgRS3l1uP+SoEg3xdRK62klTmpAtbpRa3NM2bJ9xgxtfXkgamEQGuRyzKBdiUjqdcUB0Md9xdQK69sjVQZJysomFbIhNrNFD3KxOZsYX5WNPU=";
        [HttpGet, Route("getList")]
        public string GetList()
        {
            string sql = "select * from T_VisitorAccessInf";
            //string sql = "select * from T_BlacklistInf";
            DataTable dt;
            try
            {
                dt = SQLHelper.ExecuteDataTable(sql, System.Data.CommandType.Text, null);
                return "{\"code\":1,\"data\":" + ConvertHelper.DataTableToJson(dt) + "}"; 
            }
            catch (Exception e)
            {
                return "{\"code\":0,\"msg\":\"" + new StringContent(e.ToString()) + "\"}";
            }
        }
        [HttpPost, Route("addInf")]
        public string AddInf(VisitorAccessInf v)
        {
            string wramStr = "";
            if (v.VName == "" || v.VName == null)
            {
                wramStr = "姓名不能为空";
                return "{\"code\":0,\"msg\":\"" + wramStr + "\"}";
            }
            if (v.VAddress == "" || v.VAddress == null)
            {
                wramStr = "身份证中的住址不能为空";
                return "{\"code\":0,\"msg\":\"" + wramStr + "\"}";
            }
            if (v.VCertificateNumber == "" || v.VCertificateNumber == null)
            {
                wramStr = "证件号不能为空";
                return "{\"code\":0,\"msg\":\"" + wramStr + "\"}";
            }
            
            //数据在传输过程中，密文中的“+”号会被替换合成“ ”空格号，把它还原回来
            string name = v.VName.Replace(" ","+");
            string address = v.VAddress.Replace(" ","+");
            string certificateNumber = v.VCertificateNumber.Replace(" ","+");
            
            string p = "";
            p += "VName=" + name;
            p += "&VSex=" + v.VSex;
            p += "&VNation=" + v.VNation;
            p += "&VBirthDate=" + v.VBirthDate;
            p += "&VAddress=" + address;
            p += "&VIssuingAuthority=" + v.VIssuingAuthority;
            p += "&VExpiryDate=" + v.VExpiryDate;
            p += "&VCertificatePhoto=" + v.VCertificatePhoto;
            p += "&VLocalePhoto=" + v.VLocalePhoto;
            p += "&VCertificateType=" + v.VCertificateType;
            p += "&VCertificateNumber=" + certificateNumber;
            p += "&VType=" + v.VType;
            p += "&VFromCourtId=" + v.VFromCourtId;
            p += "&VInTime=" + v.VInTime;
            p += "&VOutTime=" + v.VOutTime;
            p += "&VInPost=" + v.VInPost;
            p += "&VOutPost=" + v.VOutPost;
            p += "&VInDoorkeeper=" + v.VInDoorkeeper;
            p += "&VOutDoorkeeper=" + v.VOutDoorkeeper;
            p += "&VVisitingReason=" + v.VVisitingReason;
            p += "&VIntervieweeDept=" + v.VIntervieweeDept;
            p += "&VInterviewee=" + v.VInterviewee;
            p += "&VOffice=" + v.VOffice;
            p += "&VOfficePhone=" + v.VOfficePhone;
            p += "&VExtensionPhone=" + v.VExtensionPhone;
            p += "&VMobilePhone=" + v.VMobilePhone;
            p += "&VRemark=" + v.VRemark;

            string md5Ciphertext = v.VMD5Ciphertext;//对方传过来的所有字段的MD5密文
            //把传过来的信息再次MD5加密，和所有字段的MD5密文进行比对，保证数据在传输过程中没被修改才允许添加到数据库
            string md5P = MD5Helper._md5(p);
            if (md5Ciphertext == md5P)
            {
                string sql = "sp_addVisitorAccessInf";
                SqlParameter[] pms = new SqlParameter[]{
                    new SqlParameter("@VName",SqlDbType.NVarChar){Value=RSAHelper.DecryptWithPrivateKey(privateKey, name)},
                    new SqlParameter("@VSex",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VSex)},
                    new SqlParameter("@VNation",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VNation)},
                    new SqlParameter("@VBirthDate",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VBirthDate)},
                    new SqlParameter("@VAddress",SqlDbType.NVarChar){Value=RSAHelper.DecryptWithPrivateKey(privateKey,address)},
                    new SqlParameter("@VIssuingAuthority",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VIssuingAuthority)},
                    new SqlParameter("@VExpiryDate",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VExpiryDate)},
                    new SqlParameter("@VCertificatePhoto",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VCertificatePhoto)},
                    new SqlParameter("@VLocalePhoto",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VLocalePhoto)},
                    new SqlParameter("@VCertificateType",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VCertificateType)},
                    new SqlParameter("@VCertificateNumber",SqlDbType.NVarChar){Value=RSAHelper.DecryptWithPrivateKey(privateKey,certificateNumber)},
                    new SqlParameter("@VType",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VType)},
                    new SqlParameter("@VFromCourtId",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VFromCourtId)},
                    new SqlParameter("@VInTime",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VInTime)},
                    new SqlParameter("@VOutTime",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VOutTime)},
                    new SqlParameter("@VInPost",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VInPost)},
                    new SqlParameter("@VOutPost",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VOutPost)},
                    new SqlParameter("@VInDoorkeeper",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VInDoorkeeper)},
                    new SqlParameter("@VOutDoorkeeper",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VOutDoorkeeper)},
                    new SqlParameter("@VVisitingReason",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VVisitingReason)},
                    new SqlParameter("@VIntervieweeDept",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VIntervieweeDept)},
                    new SqlParameter("@VInterviewee",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VInterviewee)},
                    new SqlParameter("@VOffice",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VOffice)},
                    new SqlParameter("@VOfficePhone",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VOfficePhone)},
                    new SqlParameter("@VExtensionPhone",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VExtensionPhone)},
                    new SqlParameter("@VMobilePhone",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VMobilePhone)},
                    new SqlParameter("@VRemark",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VRemark)}
                };
                try
                {
                    int result = SQLHelper.ExecuteNonQuery(sql, System.Data.CommandType.StoredProcedure, pms);
                    return ConvertHelper.IntToJson(result);
                }
                catch (Exception e)
                {
                    //在webapi中要想抛出异常必须这样抛出，否则只抛出一个默认500的异常
                    var resp = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        Content = new StringContent(e.ToString()),
                        ReasonPhrase = "error"
                    };
                    throw new HttpResponseException(resp);
                }
            }
            else {
                return ConvertHelper.resultJson(0,"数据传输过程中被篡改");
            }
        }
        
        [HttpPost, Route("editInf")]
        public string EditInf(VisitorAccessInf v)
        {
            //string sql = "insert into T_VisitorAccessInf(VName, VSex, VNation, VBirthDate, VAddress, VIssuingAuthority, VExpiryDate, VCertificatePhoto, VLocalePhoto, VCertificateType, VCertificateNumber, VType, VFromCourtId, VInTime, VOutTime, VInPost, VOutPost, VInDoorkeeper, VOutDoorkeeper, VVisitingReason, VIntervieweeDept, VInterviewee, VOffice, VOfficePhone, VExtensionPhone, VMobilePhone, VRemark) values(@VName, @VSex, @VNation, @VBirthDate, @VAddress, @VIssuingAuthority, @VExpiryDate, @VCertificatePhoto, @VLocalePhoto, @VCertificateType, @VCertificateNumber, @VType, @VFromCourtId, @VInTime, @VOutTime, @VInPost, @VOutPost, @VInDoorkeeper, @VOutDoorkeeper, @VVisitingReason, @VIntervieweeDept, @VInterviewee, @VOffice, @VOfficePhone, @VExtensionPhone, @VMobilePhone, @VRemark)";
            string sql = "update T_VisitorAccessInf set VName=@VName,VSex=@VSex,VNation=@VNation,VBirthDate=@VBirthDate,VAddress=@VAddress,";
            sql += "VIssuingAuthority=@VIssuingAuthority,VExpiryDate=@VExpiryDate,VCertificatePhoto=@VCertificatePhoto,VLocalePhoto=@VLocalePhoto,VCertificateType=@VCertificateType,VCertificateNumber=@VCertificateNumber,VType=@VType,VFromCourtId=@VFromCourtId,";
            sql += "VInTime=@VInTime,VOutTime=@VOutTime,VInPost=@VInPost,VOutPost=@VOutPost,VInDoorkeeper=@VInDoorkeeper,VOutDoorkeeper=@VOutDoorkeeper,VVisitingReason=@VVisitingReason,VIntervieweeDept=@VIntervieweeDept,VInterviewee=@VInterviewee,";
            sql += "VOffice=@VOffice,VOfficePhone=@VOfficePhone,VExtensionPhone=@VExtensionPhone,VMobilePhone=@VMobilePhone,VRemark=@VRemark";
            sql += " where VId=@VId";
            SqlParameter[] pms = new SqlParameter[]{
                new SqlParameter("@VName",SqlDbType.NVarChar){Value=DataHelper.AesDecrypt(v.VName)},
                new SqlParameter("@VSex",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VSex)},
                new SqlParameter("@VNation",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VNation)},
                new SqlParameter("@VBirthDate",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VBirthDate)},
                new SqlParameter("@VAddress",SqlDbType.NVarChar){Value=DataHelper.AesDecrypt(v.VAddress)},
                new SqlParameter("@VIssuingAuthority",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VIssuingAuthority)},
                new SqlParameter("@VExpiryDate",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VExpiryDate)},
                new SqlParameter("@VCertificatePhoto",SqlDbType.NVarChar){Value=DataHelper.AesDecrypt(v.VCertificatePhoto)},
                new SqlParameter("@VLocalePhoto",SqlDbType.NVarChar){Value=DataHelper.AesDecrypt(v.VLocalePhoto)},
                new SqlParameter("@VCertificateType",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VCertificateType)},
                new SqlParameter("@VCertificateNumber",SqlDbType.NVarChar){Value=DataHelper.AesDecrypt(v.VCertificateNumber)},
                new SqlParameter("@VType",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VType)},
                new SqlParameter("@VFromCourtId",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VFromCourtId)},
                new SqlParameter("@VInTime",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VInTime)},
                new SqlParameter("@VOutTime",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VOutTime)},
                new SqlParameter("@VInPost",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VInPost)},
                new SqlParameter("@VOutPost",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VOutPost)},
                new SqlParameter("@VInDoorkeeper",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VInDoorkeeper)},
                new SqlParameter("@VOutDoorkeeper",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VOutDoorkeeper)},
                new SqlParameter("@VVisitingReason",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VVisitingReason)},
                new SqlParameter("@VIntervieweeDept",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VIntervieweeDept)},
                new SqlParameter("@VInterviewee",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VInterviewee)},
                new SqlParameter("@VOffice",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VOffice)},
                new SqlParameter("@VOfficePhone",SqlDbType.NVarChar){Value=DataHelper.AesDecrypt(v.VOfficePhone)},
                new SqlParameter("@VExtensionPhone",SqlDbType.NVarChar){Value=DataHelper.AesDecrypt(v.VExtensionPhone)},
                new SqlParameter("@VMobilePhone",SqlDbType.NVarChar){Value=DataHelper.AesDecrypt(v.VMobilePhone)},
                new SqlParameter("@VRemark",SqlDbType.NVarChar){Value=DataHelper.IsNullReturnLine(v.VRemark)},
                new SqlParameter("@VId",SqlDbType.Int){Value=v.VId}
            };
            try
            {
                int result = SQLHelper.ExecuteNonQuery(sql, System.Data.CommandType.Text, pms);
                return ConvertHelper.IntToJson(result);
            }
            catch (Exception e) {
                //在webapi中要想抛出异常必须这样抛出，否则之抛出一个默认500的异常
                var resp = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(e.ToString()),
                    ReasonPhrase = "error"
                };
                throw new HttpResponseException(resp); 
            }
            
        }
        [HttpPost, Route("deleteInfById")]
        public string DeleteInfById(VisitorAccessInf v)
        {
            //string sql = "insert into T_VisitorAccessInf(VName, VSex, VNation, VBirthDate, VAddress, VIssuingAuthority, VExpiryDate, VCertificatePhoto, VLocalePhoto, VCertificateType, VCertificateNumber, VType, VFromCourtId, VInTime, VOutTime, VInPost, VOutPost, VInDoorkeeper, VOutDoorkeeper, VVisitingReason, VIntervieweeDept, VInterviewee, VOffice, VOfficePhone, VExtensionPhone, VMobilePhone, VRemark) values(@VName, @VSex, @VNation, @VBirthDate, @VAddress, @VIssuingAuthority, @VExpiryDate, @VCertificatePhoto, @VLocalePhoto, @VCertificateType, @VCertificateNumber, @VType, @VFromCourtId, @VInTime, @VOutTime, @VInPost, @VOutPost, @VInDoorkeeper, @VOutDoorkeeper, @VVisitingReason, @VIntervieweeDept, @VInterviewee, @VOffice, @VOfficePhone, @VExtensionPhone, @VMobilePhone, @VRemark)";
            string sql = "delete from T_VisitorAccessInf where VId=@VId";
            SqlParameter[] pms = new SqlParameter[]{
                new SqlParameter("@VId",SqlDbType.Int){Value=v.VId}
            };
            try
            {
                int result = SQLHelper.ExecuteNonQuery(sql, System.Data.CommandType.Text, pms);
                return ConvertHelper.IntToJson(result);
            }
            catch (Exception e)
            {
                //在webapi中要想抛出异常必须这样抛出，否则之抛出一个默认500的异常
                var resp = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(e.ToString()),
                    ReasonPhrase = "error"
                };
                throw new HttpResponseException(resp);
            }
        }
    }
}
