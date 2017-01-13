using System;
using System.Data.SqlTypes;
using suggestionscsharp;
using Microsoft.SqlServer.Server;
using System.Collections;
using DaDataCLRSQL.Properties;

namespace SQLCalls
{
    // ПРОВЕРКИ ПЕРЕМЕННЫХ  /////////////////////////////////////////////////////////////////////////////////////////////
    public class CheckDD
    {
        public static SqlString CheckToken()
        {
            return new SqlString("DaData.ru default token is:[" + Settings.Default.dd_token + "]. To change use dbo.DaDataToken() SQL function or recompile DaDataCLRSQL.dll with new default settings.");
        }

        public static SqlString CheckURL()
        {
            return new SqlString("DaData.ru default url is:[" + Settings.Default.dd_url + "]. To change recompile DaDataCLRSQL.dll with new default settings.");
        }
    }

    // СОЕДИНЕНИЕ с DaData.ru  /////////////////////////////////////////////////////////////////////////////////////////////
    public class DDconnect
    {
        public static SuggestClient SetConnetion(string token)
        {
            if (token == "")
                token = Settings.Default.dd_token;

            return new SuggestClient(token, Settings.Default.dd_url);
        }
    }

    // ФИРМЫ ///////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class ClientApi
    {

        [SqlFunction(FillRowMethodName = "FillKlientRow",
        TableDefinition = "orgType nvarchar(15),orgStatus nvarchar(15), name nvarchar(150), nameFull nvarchar(300), mnemo nvarchar(100),managName nvarchar(100),managPost nvarchar(100),regDate smalldatetime,ligDate smalldatetime,addr nvarchar(200),opf nvarchar(100),inn nvarchar(20),kpp nvarchar(20),ogrn nvarchar(20),okpo nvarchar(20),region nvarchar(100), area nvarchar(100), city nvarchar(100), settle nvarchar(100), capitalMarker nvarchar(3),regkladrid nvarchar(50)")]
        public static IEnumerable GetClientInfo(SqlString sToken, SqlString sClientQuery, SqlByte pType, SqlByte pStatus)
        {
            var query = new PartySuggestQuery(sClientQuery.ToString());

            if  (!pType.IsNull) {
                switch ((int) pType)
                {
                    case 1:
                    query.type = PartyType.LEGAL;
                        break;
                    case 2:
                        query.type = PartyType.INDIVIDUAL;
                        break;
                    default:
                        break;
                }
            }

            if (!pStatus.IsNull)
            {
                switch ((int)pStatus)
                {
                    case 1:
                        query.status  = new PartyStatus[] { PartyStatus.ACTIVE };
                        break;
                    case 2:
                        query.status = new PartyStatus[] { PartyStatus.LIQUIDATED };
                        break;
                    case 3:
                        query.status = new PartyStatus[] { PartyStatus.LIQUIDATING };
                        break;
                    default:
                        break;
                }
            }

            var response = DDconnect.SetConnetion(sToken.ToString()).QueryParty(query);
            return response.suggestions;
        }

        public static void FillKlientRow(Object obj,
            out SqlString orgType, //.type
            out SqlString orgStatus, //.state.status.
            out SqlString name, //value
            out SqlString nameFull, //.name.full_with_opf
            out SqlString mnemo, //.name.@short
            out SqlString managName, //.management.name
            out SqlString managPost, //.management.post
            out SqlDateTime regDate, //state.registration_date
            out SqlDateTime ligDate, //.state.liquidation_date
            out SqlString addr, //.address.value
            out SqlString opf,  //.opf.@short
            out SqlString inn,  //.inn,
            out SqlString kpp,  //.kpp,
            out SqlString ogrn, //.ogrn
            out SqlString okpo, //.okpo
            out SqlString region, //.address.region
            out SqlString area, //.address.area
            out SqlString city, //address.settlement
            out SqlString settle, //.address.settlement
            out SqlString capitalMarker, //.address.capital_marker)
            out SqlString regkladrid //.address.region_kladr_id
            )
        {
            SuggestPartyResponse.Suggestions ClInfo = (SuggestPartyResponse.Suggestions)obj;

            //https://confluence.hflabs.ru/pages/viewpage.action?pageId=426639448
            // ClInfo.data.management и ClInfo.data.address - отсутствует для ИП

            orgType = new SqlString(ClInfo.data.type.ToString());
            orgStatus = new SqlString(ClInfo.data.state.status.ToString());
            name = new SqlString(ClInfo.value);
            nameFull = new SqlString(ClInfo.data.name.full_with_opf);
            mnemo = new SqlString(ClInfo.data.name.@short);
            managName = ClInfo.data.management != null ? new SqlString(ClInfo.data.management.name): new SqlString();
            managPost = ClInfo.data.management != null ? new SqlString(ClInfo.data.management.post) : new SqlString();

            var posixTime = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);
            var vtime = posixTime;

            if (!string.IsNullOrEmpty(ClInfo.data.state.registration_date))
            {
                vtime = posixTime.AddMilliseconds(long.Parse(ClInfo.data.state.registration_date));
                regDate = new SqlDateTime(vtime);
            }
            else
                regDate = new SqlDateTime();

            if (!string.IsNullOrEmpty(ClInfo.data.state.liquidation_date))
            {
                vtime = posixTime.AddMilliseconds(long.Parse(ClInfo.data.state.liquidation_date));
                ligDate = new SqlDateTime(vtime);
            }
            else
                ligDate = new SqlDateTime();

            addr = ClInfo.data.address != null ? new SqlString(ClInfo.data.address.value) : new SqlString();
            region = ClInfo.data.address != null ? new SqlString(ClInfo.data.address.region) : new SqlString();
            area = ClInfo.data.address != null ? new SqlString(ClInfo.data.address.area) : new SqlString();
            city = ClInfo.data.address != null ? new SqlString(ClInfo.data.address.city) : new SqlString();
            settle = ClInfo.data.address != null ? new SqlString(ClInfo.data.address.settlement) : new SqlString();
            capitalMarker = ClInfo.data.address != null ? new SqlString(ClInfo.data.address.capital_marker) : new SqlString();
            regkladrid = ClInfo.data.address != null ? new SqlString(ClInfo.data.address.region_kladr_id) : new SqlString();

            opf = new SqlString(ClInfo.data.opf.@short);
            inn = new SqlString(ClInfo.data.inn);
            kpp = new SqlString(ClInfo.data.kpp);
            ogrn = new SqlString(ClInfo.data.ogrn);
            okpo = new SqlString(ClInfo.data.okpo);
          
        }

    }

    // БАНКИ ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class BankApi
    {
        [SqlFunction(FillRowMethodName = "FillBankRow",
        TableDefinition = "name nvarchar(150),namePay nvarchar(150), nameFull nvarchar(300),opf nvarchar(100),bic nvarchar(30),swift nvarchar(30),okpo nvarchar(30), korr nvarchar(30),phone nvarchar(30),addr nvarchar(300),region nvarchar(100),city nvarchar(100),regkladrid nvarchar(100), regDate smalldatetime,ligDate smalldatetime,status nvarchar(30)")]

        public static IEnumerable GetBankInfo(SqlString sToken, SqlString sBankQuery, SqlByte pStatus )
        {

            var query = new BankSuggestQuery(sBankQuery.ToString());

            if (!pStatus.IsNull)
            {
                switch ((int)pStatus)
                {
                    case 1:
                        query.status = new PartyStatus[] { PartyStatus.ACTIVE };
                        break;
                    case 2:
                        query.status = new PartyStatus[] { PartyStatus.LIQUIDATED };
                        break;
                    case 3:
                        query.status = new PartyStatus[] { PartyStatus.LIQUIDATING };
                        break;
                    default:
                        break;
                }
            }

            var response = DDconnect.SetConnetion(sToken.ToString()).QueryBank(query);
            return response.suggestions;
        }

        public static void FillBankRow(Object obj,
            out SqlString name, //value
            out SqlString namePay, //.name.payment
            out SqlString nameFull, //.name.full
            out SqlString opf, //.opf.short
            out SqlString bic, //.bic
            out SqlString swift, //.swift
            out SqlString okpo, //.okpo
            out SqlString korr, //.correspondent_account
            out SqlString phone, //.phone
            out SqlString addr, //.address.value
            out SqlString region, //address.region
            out SqlString city, //address.city
            out SqlString regkladrid, //address.region_kladr_id
            out SqlDateTime regDate, //.state.registration_date
            out SqlDateTime ligDate, //.state.liquidation_date
            out SqlString status //state.status
             )
        {
            SuggestBankResponse.Suggestions ClInfo = (SuggestBankResponse.Suggestions)obj;
            //https://confluence.hflabs.ru/pages/viewpage.action?pageId=426639462

            name = new SqlString(ClInfo.value);
            nameFull = new SqlString(ClInfo.data.name.full);
            namePay = new SqlString(ClInfo.data.name.payment);
            opf = new SqlString(ClInfo.data.opf.@short);
            bic = new SqlString(ClInfo.data.bic);
            swift = new SqlString(ClInfo.data.swift);
            okpo = new SqlString(ClInfo.data.okpo);
            korr = new SqlString(ClInfo.data.correspondent_account);
            phone = new SqlString(ClInfo.data.phone);

            addr = new SqlString(ClInfo.data.address.value);
            region = new SqlString(ClInfo.data.address.region);
            city = new SqlString(ClInfo.data.address.city);
            regkladrid = new SqlString(ClInfo.data.address.region_kladr_id);


            var posixTime = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);
            var vtime = posixTime;

            if (!string.IsNullOrEmpty(ClInfo.data.state.registration_date))
            {
                vtime = posixTime.AddMilliseconds(long.Parse(ClInfo.data.state.registration_date));
                regDate = new SqlDateTime(vtime);
            }
            else
                regDate = new SqlDateTime();

            if (!string.IsNullOrEmpty(ClInfo.data.state.liquidation_date))
            {
                vtime = posixTime.AddMilliseconds(long.Parse(ClInfo.data.state.liquidation_date));
                ligDate = new SqlDateTime(vtime);
            }
            else
                ligDate = new SqlDateTime();

            status = new SqlString(ClInfo.data.state.status.ToString());
        }

    }

    // АДРЕСА ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class AdressApi
    {
        [SqlFunction(FillRowMethodName = "FillAdressRow",
        TableDefinition = "addr nvarchar(200),postalCode nvarchar(10),country nvarchar(50),region nvarchar(100),area nvarchar(100),city nvarchar(100),settle nvarchar(100),street nvarchar(100),house  nvarchar(10), house_type nvarchar(50), block nvarchar(10),block_type nvarchar(50),flat  nvarchar(10),flat_type nvarchar(50),timezone nvarchar(5),capitalMarker nvarchar(5),regkladrid nvarchar(50),fias_level nvarchar(5),kladrid nvarchar(50), geo_lat nvarchar(30), geo_lon nvarchar(30), qc_geo nvarchar(1)")]

        public static IEnumerable GetAdressInfo(SqlString sToken, SqlString sAdressQuery, SqlString pLocID, SqlString pBoundFrom, SqlString pBoundTo)
        {
            var query = new AddressSuggestQuery(sAdressQuery.ToString());

            if (! pLocID.IsNull )
                {
                var location = new AddressData();
                location.kladr_id = pLocID.ToString();
                query.locations = new AddressData[] { location };
                }
            
            if (!pBoundFrom.IsNull & !pBoundTo.IsNull )
                {
                query.from_bound = new AddressBound(pBoundFrom.ToString ());
                query.to_bound = new AddressBound(pBoundTo.ToString());
            }
            
            var response = DDconnect.SetConnetion(sToken.ToString()).QueryAddress(query);
            return response.suggestions;
        }

        public static void FillAdressRow(Object obj,
            out SqlString addr, //value
            out SqlString postalCode,
            out SqlString country,
            out SqlString region,
            out SqlString area,
            out SqlString city,
            out SqlString settle,
            out SqlString street,
            out SqlString house,
            out SqlString house_type,
            out SqlString block,
            out SqlString block_type,
            out SqlString flat,
            out SqlString flat_type,
            out SqlString timezone,
            out SqlString capitalMarker,
            out SqlString regkladrid,
            out SqlString fias_level,
            out SqlString kladrid,
            out SqlString geo_lat,
            out SqlString geo_lon,
            out SqlString qc_geo
            )
        {
            SuggestAddressResponse.Suggestions ClInfo = (SuggestAddressResponse.Suggestions)obj;

            //https://confluence.hflabs.ru/pages/viewpage.action?pageId=426639431

            addr = new SqlString(ClInfo.value);
            postalCode = new SqlString(ClInfo.data.postal_code);
            country = new SqlString(ClInfo.data.country);
            region = new SqlString(ClInfo.data.region_with_type);
            area = new SqlString(ClInfo.data.area);
            city = new SqlString(ClInfo.data.city);
            settle = new SqlString(ClInfo.data.settlement);
            street = new SqlString(ClInfo.data.street_with_type);
            house = new SqlString(ClInfo.data.house);
            house_type = new SqlString(ClInfo.data.house_type);
            block = new SqlString(ClInfo.data.block);
            block_type = new SqlString(ClInfo.data.block_type);
            flat = new SqlString(ClInfo.data.flat);
            flat_type = new SqlString(ClInfo.data.flat_type);
            timezone = new SqlString(ClInfo.data.timezone);
            capitalMarker = new SqlString(ClInfo.data.capital_marker);
            regkladrid = new SqlString(ClInfo.data.region_kladr_id);
            fias_level = new SqlString(ClInfo.data.fias_level);
            kladrid = new SqlString(ClInfo.data.kladr_id);
            geo_lat = new SqlString(ClInfo.data.geo_lat);
            geo_lon = new SqlString(ClInfo.data.geo_lon);
            qc_geo = new SqlString(ClInfo.data.qc_geo);

        }

    }

    // EMAILS ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class EmailApi
    {
        [SqlFunction(FillRowMethodName = "FillEmailRow",
        TableDefinition = "email nvarchar(50),local nvarchar(30),domain nvarchar(20)")]

        public static IEnumerable GetEmailInfo(SqlString sToken, SqlString sEmailQuery)
        {
            var response = DDconnect.SetConnetion(sToken.ToString()).QueryEmail(sEmailQuery.ToString());
            return response.suggestions;
        }

        public static void FillEmailRow(Object obj,
            out SqlString email, //value
            out SqlString local,
            out SqlString domain
            )
        {
            SuggestEmailResponse.Suggestions ClInfo = (SuggestEmailResponse.Suggestions)obj;

            //https://confluence.hflabs.ru/pages/viewpage.action?pageId=426639456

            email = new SqlString(ClInfo.value);
            local = new SqlString(ClInfo.data.local);
            domain = new SqlString(ClInfo.data.domain);
        }

    }
   
    // NAMES ///////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class NamesApi
    {
        [SqlFunction(FillRowMethodName = "FillNameRow",
        TableDefinition = "FullName nvarchar(300), Name nvarchar(50), Middlename nvarchar(50), Surname nvarchar(50),  Gender nvarchar(10)")]

        public static IEnumerable GetNameInfo(SqlString sToken, SqlString sNameQuery, SqlByte pFioPart)
        {
            var query = new FioSuggestQuery(sNameQuery.ToString());

            if (!pFioPart.IsNull)
            {
                switch ((int)pFioPart)
                {
                    case 1:
                        query.parts = new FioPart[] { FioPart.NAME };
                        break;
                    case 2:
                        query.parts = new FioPart[] { FioPart.PATRONYMIC };
                        break;
                    case 3:
                        query.parts = new FioPart[] { FioPart.SURNAME };
                        break;
                    default:
                        break;
                }
            }

            var response = DDconnect.SetConnetion(sToken.ToString()).QueryFio(query);
            return response.suggestions;
        }

        public static void FillNameRow(Object obj,
            out SqlString FullName, //value
            out SqlString Name, //name
            out SqlString Middlename, //patronymic
            out SqlString Surname, //surname
            out SqlString Gender //gender
            )
        {
            SuggestFioResponse.Suggestions ClInfo = (SuggestFioResponse.Suggestions)obj;

            //https://confluence.hflabs.ru/pages/viewpage.action?pageId=426639442
            
            FullName = new SqlString(ClInfo.value);
            Name = new SqlString(ClInfo.data.name);
            Middlename = new SqlString(ClInfo.data.patronymic);
            Surname = new SqlString(ClInfo.data.surname);
            Gender = new SqlString(ClInfo.data.gender.ToString());

        }

    }

}
