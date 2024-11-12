/* Options:
Date: 2024-11-12 15:13:01
Version: 8.40
Tip: To override a DTO option, remove "//" prefix before updating
BaseUrl: https://localhost:5001

//AddServiceStackTypes: True
//AddDocAnnotations: True
//AddDescriptionAsComments: True
//IncludeTypes: 
//ExcludeTypes: 
//DefaultImports: 
*/

"use strict";
/** @typedef {'Initial'|'Sent'|'Delivered'|'Failed'} */
export var MessageStatus;
(function (MessageStatus) {
    MessageStatus["Initial"] = "Initial"
    MessageStatus["Sent"] = "Sent"
    MessageStatus["Delivered"] = "Delivered"
    MessageStatus["Failed"] = "Failed"
})(MessageStatus || (MessageStatus = {}));
export class QueryBase {
    /** @param {{skip?:number,take?:number,orderBy?:string,orderByDesc?:string,include?:string,fields?:string,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {?number} */
    skip;
    /** @type {?number} */
    take;
    /** @type {string} */
    orderBy;
    /** @type {string} */
    orderByDesc;
    /** @type {string} */
    include;
    /** @type {string} */
    fields;
    /** @type {{ [index: string]: string; }} */
    meta;
}
/** @typedef T {any} */
export class QueryDb extends QueryBase {
    /** @param {{skip?:number,take?:number,orderBy?:string,orderByDesc?:string,include?:string,fields?:string,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { super(init); Object.assign(this, init) }
}
export class AuditBase {
    /** @param {{createdDate?:string,createdBy?:string,modifiedDate?:string,modifiedBy?:string,deletedDate?:string,deletedBy?:string}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    createdDate;
    /** @type {string} */
    createdBy;
    /** @type {string} */
    modifiedDate;
    /** @type {string} */
    modifiedBy;
    /** @type {?string} */
    deletedDate;
    /** @type {string} */
    deletedBy;
}
/** @typedef {'Initial'|'Active'|'Cancel'|'Locked'} */
export var MessageTemplateStatus;
(function (MessageTemplateStatus) {
    MessageTemplateStatus["Initial"] = "Initial"
    MessageTemplateStatus["Active"] = "Active"
    MessageTemplateStatus["Cancel"] = "Cancel"
    MessageTemplateStatus["Locked"] = "Locked"
})(MessageTemplateStatus || (MessageTemplateStatus = {}));
/** @typedef {'Inactive'|'Active'} */
export var PartnerStatus;
(function (PartnerStatus) {
    PartnerStatus["Inactive"] = "Inactive"
    PartnerStatus["Active"] = "Active"
})(PartnerStatus || (PartnerStatus = {}));
export class Partner extends AuditBase {
    /** @param {{id?:number,partnerCode?:string,partnerName?:string,emailAddress?:string,phoneNumber?:string,apiKey?:string,userName?:string,password?:string,ips?:string,status?:PartnerStatus,createdDate?:string,createdBy?:string,modifiedDate?:string,modifiedBy?:string,deletedDate?:string,deletedBy?:string}} [init] */
    constructor(init) { super(init); Object.assign(this, init) }
    /** @type {number} */
    id;
    /** @type {string} */
    partnerCode;
    /** @type {string} */
    partnerName;
    /** @type {string} */
    emailAddress;
    /** @type {string} */
    phoneNumber;
    /** @type {string} */
    apiKey;
    /** @type {string} */
    userName;
    /** @type {string} */
    password;
    /** @type {string} */
    ips;
    /** @type {PartnerStatus} */
    status;
}
export class MessageTemplate extends AuditBase {
    /** @param {{id?:number,content?:string,status?:MessageTemplateStatus,partnerId?:number,partnerName?:Partner,createdDate?:string,createdBy?:string,modifiedDate?:string,modifiedBy?:string,deletedDate?:string,deletedBy?:string}} [init] */
    constructor(init) { super(init); Object.assign(this, init) }
    /** @type {number} */
    id;
    /** @type {string} */
    content;
    /** @type {MessageTemplateStatus} */
    status;
    /** @type {number} */
    partnerId;
    /** @type {Partner} */
    partnerName;
}
/** @typedef {'Inactive'|'Active'} */
export var ProviderStatus;
(function (ProviderStatus) {
    ProviderStatus["Inactive"] = "Inactive"
    ProviderStatus["Active"] = "Active"
})(ProviderStatus || (ProviderStatus = {}));
export class Provider extends AuditBase {
    /** @param {{id?:number,providerCode?:string,providerName?:string,emailAddress?:string,phoneNumber?:string,apiKey?:string,userName?:string,password?:string,apiUrl?:string,status?:ProviderStatus,createdDate?:string,createdBy?:string,modifiedDate?:string,modifiedBy?:string,deletedDate?:string,deletedBy?:string}} [init] */
    constructor(init) { super(init); Object.assign(this, init) }
    /** @type {number} */
    id;
    /** @type {string} */
    providerCode;
    /** @type {string} */
    providerName;
    /** @type {string} */
    emailAddress;
    /** @type {string} */
    phoneNumber;
    /** @type {string} */
    apiKey;
    /** @type {string} */
    userName;
    /** @type {string} */
    password;
    /** @type {string} */
    apiUrl;
    /** @type {ProviderStatus} */
    status;
}
export class Message extends AuditBase {
    /** @param {{id?:number,sms?:string,status?:MessageStatus,receiver?:string,messageTemplate?:MessageTemplate,messageTemplateId?:number,partnerId?:number,requestId?:string,partner?:Partner,providerId?:number,provider?:Provider,requestDate?:string,sentDate?:string,responseDate?:string,telco?:string,responseMassage?:string,messageId?:string,createdDate?:string,createdBy?:string,modifiedDate?:string,modifiedBy?:string,deletedDate?:string,deletedBy?:string}} [init] */
    constructor(init) { super(init); Object.assign(this, init) }
    /** @type {number} */
    id;
    /** @type {string} */
    sms;
    /** @type {MessageStatus} */
    status;
    /** @type {string} */
    receiver;
    /** @type {MessageTemplate} */
    messageTemplate;
    /** @type {number} */
    messageTemplateId;
    /** @type {number} */
    partnerId;
    /** @type {string} */
    requestId;
    /** @type {Partner} */
    partner;
    /** @type {number} */
    providerId;
    /** @type {Provider} */
    provider;
    /** @type {string} */
    requestDate;
    /** @type {?string} */
    sentDate;
    /** @type {?string} */
    responseDate;
    /** @type {string} */
    telco;
    /** @type {string} */
    responseMassage;
    /** @type {string} */
    messageId;
}
export class MessageTemplateDetail {
    /** @param {{id?:number,messageTemplateId?:number,messageTemplate?:MessageTemplate,content?:string}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {number} */
    id;
    /** @type {number} */
    messageTemplateId;
    /** @type {MessageTemplate} */
    messageTemplate;
    /** @type {string} */
    content;
}
export class ResponseError {
    /** @param {{errorCode?:string,fieldName?:string,message?:string,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    errorCode;
    /** @type {string} */
    fieldName;
    /** @type {string} */
    message;
    /** @type {{ [index: string]: string; }} */
    meta;
}
export class ResponseStatus {
    /** @param {{errorCode?:string,message?:string,stackTrace?:string,errors?:ResponseError[],meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    errorCode;
    /** @type {string} */
    message;
    /** @type {string} */
    stackTrace;
    /** @type {ResponseError[]} */
    errors;
    /** @type {{ [index: string]: string; }} */
    meta;
}
export class AuthenticateResponse {
    /** @param {{userId?:string,sessionId?:string,userName?:string,displayName?:string,referrerUrl?:string,bearerToken?:string,refreshToken?:string,refreshTokenExpiry?:string,profileUrl?:string,roles?:string[],permissions?:string[],responseStatus?:ResponseStatus,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    userId;
    /** @type {string} */
    sessionId;
    /** @type {string} */
    userName;
    /** @type {string} */
    displayName;
    /** @type {string} */
    referrerUrl;
    /** @type {string} */
    bearerToken;
    /** @type {string} */
    refreshToken;
    /** @type {?string} */
    refreshTokenExpiry;
    /** @type {string} */
    profileUrl;
    /** @type {string[]} */
    roles;
    /** @type {string[]} */
    permissions;
    /** @type {ResponseStatus} */
    responseStatus;
    /** @type {{ [index: string]: string; }} */
    meta;
}
/** @typedef T {any} */
export class QueryResponse {
    /** @param {{offset?:number,total?:number,results?:T[],meta?:{ [index: string]: string; },responseStatus?:ResponseStatus}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {number} */
    offset;
    /** @type {number} */
    total;
    /** @type {T[]} */
    results;
    /** @type {{ [index: string]: string; }} */
    meta;
    /** @type {ResponseStatus} */
    responseStatus;
}
export class IdResponse {
    /** @param {{id?:string,responseStatus?:ResponseStatus}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    id;
    /** @type {ResponseStatus} */
    responseStatus;
}
export class CreateSendMessageRequest {
    /** @param {{id?:number,sms?:string,receiver?:string,telco?:string,providerId?:number,partnerCode?:string,requestId?:string}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {number} */
    id;
    /** @type {string} */
    sms;
    /** @type {string} */
    receiver;
    /** @type {string} */
    telco;
    /** @type {number} */
    providerId;
    /** @type {string} */
    partnerCode;
    /** @type {string} */
    requestId;
    getTypeName() { return 'CreateSendMessageRequest' }
    getMethod() { return 'POST' }
    createResponse () { };
}
export class Authenticate {
    /** @param {{provider?:string,userName?:string,password?:string,rememberMe?:boolean,accessToken?:string,accessTokenSecret?:string,returnUrl?:string,errorView?:string,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { Object.assign(this, init) }
    /**
     * @type {string}
     * @description AuthProvider, e.g. credentials */
    provider;
    /** @type {string} */
    userName;
    /** @type {string} */
    password;
    /** @type {?boolean} */
    rememberMe;
    /** @type {string} */
    accessToken;
    /** @type {string} */
    accessTokenSecret;
    /** @type {string} */
    returnUrl;
    /** @type {string} */
    errorView;
    /** @type {{ [index: string]: string; }} */
    meta;
    getTypeName() { return 'Authenticate' }
    getMethod() { return 'POST' }
    createResponse() { return new AuthenticateResponse() }
}
export class QueryMessages extends QueryDb {
    /** @param {{id?:number,status?:MessageStatus,receiver?:string,telco?:string,fromDate?:string,toDate?:string,skip?:number,take?:number,orderBy?:string,orderByDesc?:string,include?:string,fields?:string,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { super(init); Object.assign(this, init) }
    /** @type {?number} */
    id;
    /** @type {?MessageStatus} */
    status;
    /** @type {string} */
    receiver;
    /** @type {string} */
    telco;
    /** @type {?string} */
    fromDate;
    /** @type {?string} */
    toDate;
    getTypeName() { return 'QueryMessages' }
    getMethod() { return 'GET' }
    createResponse() { return new QueryResponse() }
}
export class QueryMessageTemplates extends QueryDb {
    /** @param {{id?:number,skip?:number,take?:number,orderBy?:string,orderByDesc?:string,include?:string,fields?:string,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { super(init); Object.assign(this, init) }
    /** @type {?number} */
    id;
    getTypeName() { return 'QueryMessageTemplates' }
    getMethod() { return 'GET' }
    createResponse() { return new QueryResponse() }
}
export class QueryMessageTemplatesByStatus extends QueryDb {
    /** @param {{status?:MessageTemplateStatus,skip?:number,take?:number,orderBy?:string,orderByDesc?:string,include?:string,fields?:string,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { super(init); Object.assign(this, init) }
    /** @type {MessageTemplateStatus} */
    status;
    getTypeName() { return 'QueryMessageTemplatesByStatus' }
    getMethod() { return 'GET' }
    createResponse() { return new QueryResponse() }
}
export class QueryMessageTemplateDetails extends QueryDb {
    /** @param {{id?:number,skip?:number,take?:number,orderBy?:string,orderByDesc?:string,include?:string,fields?:string,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { super(init); Object.assign(this, init) }
    /** @type {?number} */
    id;
    getTypeName() { return 'QueryMessageTemplateDetails' }
    getMethod() { return 'GET' }
    createResponse() { return new QueryResponse() }
}
export class GetPartners extends QueryDb {
    /** @param {{id?:number,skip?:number,take?:number,orderBy?:string,orderByDesc?:string,include?:string,fields?:string,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { super(init); Object.assign(this, init) }
    /** @type {?number} */
    id;
    getTypeName() { return 'GetPartners' }
    getMethod() { return 'GET' }
    createResponse() { return new QueryResponse() }
}
export class GetProviders extends QueryDb {
    /** @param {{id?:number,skip?:number,take?:number,orderBy?:string,orderByDesc?:string,include?:string,fields?:string,meta?:{ [index: string]: string; }}} [init] */
    constructor(init) { super(init); Object.assign(this, init) }
    /** @type {?number} */
    id;
    getTypeName() { return 'GetProviders' }
    getMethod() { return 'GET' }
    createResponse() { return new QueryResponse() }
}
export class CreateMessageTemplate {
    /** @param {{content?:string,status?:MessageTemplateStatus,partnerName?:number}} [init] */
    constructor(init) { Object.assign(this, init) }
    /**
     * @type {string}
     * @description Missing template */
    content;
    /** @type {MessageTemplateStatus} */
    status;
    /** @type {?number} */
    partnerName;
    getTypeName() { return 'CreateMessageTemplate' }
    getMethod() { return 'POST' }
    createResponse() { return new IdResponse() }
}
export class UpdateMessageTemplate {
    /** @param {{id?:number,content?:string,status?:MessageTemplateStatus}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {number} */
    id;
    /**
     * @type {string}
     * @description Missing template */
    content;
    /** @type {MessageTemplateStatus} */
    status;
    getTypeName() { return 'UpdateMessageTemplate' }
    getMethod() { return 'PATCH' }
    createResponse() { return new IdResponse() }
}
export class DeleteMessageTemplate {
    /** @param {{id?:number}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {number} */
    id;
    getTypeName() { return 'DeleteMessageTemplate' }
    getMethod() { return 'DELETE' }
    createResponse() { }
}
export class UpdateMessageTemplateDetail {
    /** @param {{id?:number,providerId?:number,content?:string}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {number} */
    id;
    /** @type {number} */
    providerId;
    /** @type {string} */
    content;
    getTypeName() { return 'UpdateMessageTemplateDetail' }
    getMethod() { return 'PATCH' }
    createResponse() { return new IdResponse() }
}
export class DeleteMessageTemplateDetail {
    /** @param {{id?:number}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {number} */
    id;
    getTypeName() { return 'DeleteMessageTemplateDetail' }
    getMethod() { return 'DELETE' }
    createResponse() { }
}
export class CreatePartner {
    /** @param {{partnerCode?:string,partnerName?:string,emailAddress?:string,phoneNumber?:string,apiKey?:string,userName?:string,password?:string,ips?:string,status?:PartnerStatus}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    partnerCode;
    /** @type {string} */
    partnerName;
    /** @type {string} */
    emailAddress;
    /** @type {string} */
    phoneNumber;
    /** @type {string} */
    apiKey;
    /** @type {string} */
    userName;
    /** @type {string} */
    password;
    /** @type {string} */
    ips;
    /** @type {PartnerStatus} */
    status;
    getTypeName() { return 'CreatePartner' }
    getMethod() { return 'POST' }
    createResponse() { return new IdResponse() }
}
export class UpdatePartner {
    /** @param {{id?:number,partnerCode?:string,partnerName?:string,emailAddress?:string,phoneNumber?:string,apiKey?:string,userName?:string,password?:string,ips?:string,status?:PartnerStatus}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {number} */
    id;
    /** @type {string} */
    partnerCode;
    /** @type {string} */
    partnerName;
    /** @type {string} */
    emailAddress;
    /** @type {string} */
    phoneNumber;
    /** @type {string} */
    apiKey;
    /** @type {string} */
    userName;
    /** @type {string} */
    password;
    /** @type {string} */
    ips;
    /** @type {PartnerStatus} */
    status;
    getTypeName() { return 'UpdatePartner' }
    getMethod() { return 'PATCH' }
    createResponse() { return new IdResponse() }
}
export class DeletePartner {
    /** @param {{id?:number}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {number} */
    id;
    getTypeName() { return 'DeletePartner' }
    getMethod() { return 'DELETE' }
    createResponse() { }
}
export class CreateProvider {
    /** @param {{providerCode?:string,providerName?:string,emailAddress?:string,phoneNumber?:string,apiKey?:string,userName?:string,password?:string,apiUrl?:string,status?:ProviderStatus}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {string} */
    providerCode;
    /** @type {string} */
    providerName;
    /** @type {string} */
    emailAddress;
    /** @type {string} */
    phoneNumber;
    /** @type {string} */
    apiKey;
    /** @type {string} */
    userName;
    /** @type {string} */
    password;
    /** @type {string} */
    apiUrl;
    /** @type {ProviderStatus} */
    status;
    getTypeName() { return 'CreateProvider' }
    getMethod() { return 'POST' }
    createResponse() { return new IdResponse() }
}
export class UpdateProvider {
    /** @param {{id?:number,providerCode?:string,providerName?:string,emailAddress?:string,phoneNumber?:string,apiKey?:string,userName?:string,password?:string,apiUrl?:string,status?:ProviderStatus}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {number} */
    id;
    /** @type {string} */
    providerCode;
    /** @type {string} */
    providerName;
    /** @type {string} */
    emailAddress;
    /** @type {string} */
    phoneNumber;
    /** @type {string} */
    apiKey;
    /** @type {string} */
    userName;
    /** @type {string} */
    password;
    /** @type {string} */
    apiUrl;
    /** @type {ProviderStatus} */
    status;
    getTypeName() { return 'UpdateProvider' }
    getMethod() { return 'PATCH' }
    createResponse() { return new IdResponse() }
}
export class DeleteProvider {
    /** @param {{id?:number}} [init] */
    constructor(init) { Object.assign(this, init) }
    /** @type {number} */
    id;
    getTypeName() { return 'DeleteProvider' }
    getMethod() { return 'DELETE' }
    createResponse() { }
}

