var buttonConfig = null;
var userConfig = null;
var defaultTeams = null;
var forceFormRefresh = false;
var currentUserConfig = new Array();
var amConfig = new Array();
var statusNullAlert = false;
function WarnUserIfFormIsDirty() {
    if (Xrm.Page.data.entity.getIsDirty()) {
        return confirm("There are some changes on the form. Clicking this button will cause losing them. \nAre you sure to continue without saving those changes ?");
    }
    return true;
}
//======================================================================================================================
// EventSource:   on click of copy entry button on ribbon
// Functionality: dublicates selected entries
// Requirements : pass guid of selected entries to dublicateRecord method.
// Yazar:         EE
//======================================================================================================================
function DuplicateRecord(EntityId, LogicalEntityName) {

    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty()) {
        return;
    }

    //openDialog
    var loop;
    if (loop == null) {
        var funcName = "DuplicateRecord";
        var addParams = "EntityId=" + EntityId + "&LogicalEntityName=" + LogicalEntityName + "&funcName=" + funcName;
        var src = "/webresources/Html/CustomDataOperationsRecords.htm?Data=" + encodeURIComponent(addParams);
        var DialogOptions = new Xrm.DialogOptions();
        DialogOptions.width = 1024;
        DialogOptions.height = 768;
        Xrm.Internal.openDialog(src, DialogOptions, null, null, CallbackFunction);
        
        function CallbackFunction(returnValue) {
            //TODO something with the returned value
            loop = 0;
        }
    }

    var entId = "";
    if (Array.isArray(EntityId) && EntityId.length > 0) {
        entId = EntityId[0];
    }
    else
        entId = EntityId;
    $jq.ajax({
        type: "GET",
        url: helperServiceUrl + "DuplicateSingleRecord",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        async: false,
        //crossDomain: true,
        data: { LogicalEntityName: LogicalEntityName, itemGuid: entId, userId: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        if (checkCopyResults(data.d))
            Xrm.Utility.openEntityForm(Xrm.Page.data.entity.getEntityName(), data.d, null);
    }).fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function DuplicateMultipleRecords(EntityTypeName, EntityIds, sugGridName) {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (EntityIds.length > 0) {
        if (EntityIds.length > 30) {
            alert("You can select maximum 30 records !");
            return;
        }
        $jq.ajax({
            type: "GET",
            url: helperServiceUrl + "DuplicateMultipleRecords",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            cache: false,
            async: false,
            //crossDomain: true,
            data: { LogicalEntityName: EntityTypeName, itemGuids: JSON.stringify(EntityIds), userId: currentUser }
        }).done(function (data, textStatus, XmlHttpRequest) {
            if (checkCopyResults(data.d))
                alert("Selected records duplicated !");
        }).fail(function (XmlHttpRequest, textStatus, errorThrown) {
            failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
        })
    .always(function (data, textStatus, XmlHttpRequest) {
        sugGridName.refresh();
    });
    }
    else {
        alert("Please select at least one service !");
    }
}

function DuplicateAlternativeForRecord(EntityId, LogicalEntityName) {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var entId = "";
    if (Array.isArray(EntityId) && EntityId.length > 0) {
        entId = EntityId[0];
    }
    else
        entId = EntityId;
    $jq.ajax({
        type: "GET",
        url: helperServiceUrl + "AlternativeFor",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        async: false,
        //crossDomain: true,
        data: { LogicalEntityName: LogicalEntityName, itemGuid: entId, userId: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        if (checkCopyResults(data.d))
            Xrm.Utility.openEntityForm(Xrm.Page.data.entity.getEntityName(), data.d, null);
    }).fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function DuplicateMultipleAlternativeForRecord(EntityTypeName, EntityIds, sugGridName) {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (EntityIds.length > 0) {
        $jq.ajax({
            type: "GET",
            url: helperServiceUrl + "AlternateMultipleRecords",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            cache: false,
            async: false,
            //crossDomain: true,
            data: { LogicalEntityName: EntityTypeName, itemGuids: JSON.stringify(EntityIds), userId: currentUser }
        }).done(function (data, textStatus, XmlHttpRequest) {
            if (checkCopyResults(data.d))
                alert("Selected records duplicated !");
        }).fail(function (XmlHttpRequest, textStatus, errorThrown) {
            failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
        })
        .always(function (data, textStatus, XmlHttpRequest) {
            sugGridName.refresh();
        });
    }
    else {
        alert("Please select at least one service !");
    }
}

function cBudgetaryToFirm(CmdProperties) {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var controls = CmdProperties.SourceControlId.split("|");
    var btnName = controls[controls.length - 1];

    $jq.ajax({
        type: "GET",
        url: helperServiceUrl + "ConvertBtoF",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        async: false,
        //crossDomain: true,
        data: { budgetaryOfferId: Xrm.Page.data.entity.getId(), userid: currentUser, buttonName: btnName }
    }).done(function (data, textStatus, XmlHttpRequest) {
        //call open entity form method in global scripts
        Xrm.Utility.openEntityForm("quote", data.d, null);
        window.onbeforeunload = null;
        Xrm.Page.ui.close();
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function cLeadToCustomers() { // Lead to Customer
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    $jq.ajax({
        type: "GET",
        url: helperServiceUrl + "ConvertLtoC",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        async: false,
        //crossDomain: true,
        data: { leadId: Xrm.Page.data.entity.getId(), userid: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        Xrm.Utility.openEntityForm("account", data.d, null);
        window.onbeforeunload = null;
        Xrm.Page.ui.close();
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function cRejectLead() { // Reject Lead
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    Xrm.Page.getAttribute("new_rejectdescription").setSubmitMode("always");
    Xrm.Page.data.entity.save();
    $jq.ajax({
        type: "GET",
        url: helperServiceUrl + "RejectLead",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        async: false,
        //crossDomain: true,
        data: { leadId: Xrm.Page.data.entity.getId() }
    }).done(function (data, textStatus, XmlHttpRequest) {
        alert("Lead rejected !");
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function cFirmToOrder(CmdProperties) {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var controls = CmdProperties.SourceControlId.split("|");
    var btnName = controls[controls.length - 1];

    $jq.ajax({
        type: "GET",
        url: helperServiceUrl + "ConvertFtoO",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        async: false,
        //crossDomain: true,
        data: { firmOfferId: Xrm.Page.data.entity.getId(), userid: currentUser, buttonName: btnName }
    }).done(function (data, textStatus, XmlHttpRequest) {
        //call open entity form method in global scripts
        Xrm.Utility.openEntityForm("salesorder", data.d, null);
        window.onbeforeunload = null;
        Xrm.Page.ui.close();
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function cSuppBudgertaryToSuppFirm(CmdProperties) {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var controls = CmdProperties.SourceControlId.split("|");
    var btnName = controls[controls.length - 1];

    $jq.ajax({
        type: "GET",
        url: helperServiceUrl + "ConvertSBtoSF",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        async: false,
        //crossDomain: true,
        data: { supplierBudgetaryOfferId: Xrm.Page.data.entity.getId(), userid: currentUser, buttonName: btnName }
    }).done(function (data, textStatus, XmlHttpRequest) {
        //call open entity form method in global scripts
        Xrm.Utility.openEntityForm("new_supplierfirmoffer", data.d, null);
        window.onbeforeunload = null;
        Xrm.Page.ui.close();
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function cSuppFirmToSuppContract(CmdProperties) {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var controls = CmdProperties.SourceControlId.split("|");
    var btnName = controls[controls.length - 1];

    $jq.ajax({
        type: "GET",
        url: helperServiceUrl + "ConvertSFtoSC",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        async: false,
        //crossDomain: true,
        data: { supplierFirmOfferId: Xrm.Page.data.entity.getId(), userid: currentUser, buttonName: btnName }
    }).done(function (data, textStatus, XmlHttpRequest) {
        //call open entity form method in global scripts
        Xrm.Utility.openEntityForm("new_suppliercontract", data.d, null);
        window.onbeforeunload = null;
        Xrm.Page.ui.close();
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function getMultiplePrices(subGridName, CmdProperties) {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var commercialRelationGridControl = document.getElementById(subGridName).control;
    var ids = commercialRelationGridControl.get_selectedIds();

    if (commercialRelationGridControl.get_allRecordIds().length > 0) {
        var entName = getEntityLogicalName();
        var controls = CmdProperties.SourceControlId.split("|");
        var btnName = controls[controls.length - 1];
        $jq.ajax({
            type: "GET",
            url: helperServiceUrl + "GetWholePrices",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            cache: false,
            async: false,
            //crossDomain: true,
            data: { offerId: getCurrentEntityId(), OfferTypeName: entName, buttonName: btnName, userId: currentUser }
        }).done(function (data, textStatus, XmlHttpRequest) {
            if (data != null && typeof (data.d) != "undefined") {
                if (data.d == null) {
                    alert("Service prices are fetched !");
                    forceFormRefresh = true;
                    window.onbeforeunload = null;
                    window.location.reload(true);
                }
                else if (data.d.length == 0) {
                    alert("Supplier offers were generated for this offer. This case will be forwarded to SMT!");
                    forceFormRefresh = true;
                    window.onbeforeunload = null;
                    window.location.reload(true);
                }
                else {
                    var dCount = 0;
                    var valError = false;
                    var priceNotFoundError = false;
                    //In the background first validation errors are checked. If there are validation problems,
                    //prices won't be checked. So it is not possible to have two error messages at the same time.
                    var cMsg = "Required fields were not filled for these services :";
                    var pMsg = "Prices could not be found for these services :";
                    for (dCount = 0; dCount < data.d.length; dCount++) {
                        if (data.d[dCount].ValidationFields.length > 0) {
                            cMsg += "\n" + data.d[dCount].ServiceNo;
                            valError = true;
                        }
                        if (data.d[dCount].InstallationFee == -1 && data.d[dCount].MonthlyFee == -1) {
                            pMsg += "\n" + data.d[dCount].ServiceNo;
                            priceNotFoundError = true;
                        }
                    }
                    cMsg += "\nWould you like to see more details ?";
                    if (valError) {
                        if (confirm(cMsg)) {
                            var dMsg = "";
                            for (dCount = 0; dCount < data.d.length; dCount++) {
                                if (data.d[dCount].ValidationFields.length > 0) {
                                    dMsg += data.d[dCount].ServiceNo;
                                    var moreInfo = data.d[dCount].ValidationFields;
                                    var fCount = 0;
                                    for (fCount = 0; fCount < moreInfo[0].Value.length; fCount++) {
                                        dMsg += "\n     " + moreInfo[0].Value[fCount];
                                    }
                                    dMsg += "\n";
                                }
                            }
                            alert(dMsg);
                        }
                    }
                    if (priceNotFoundError && !valError) {
                        pMsg += "\nCase will be forwarded to Supplier Management !";
                        alert(pMsg);
                        forceFormRefresh = true;
                        window.onbeforeunload = null;
                        window.location.reload(true);
                    }
                }
            }
        })
        .fail(function (XmlHttpRequest, textStatus, errorThrown) {
            failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
            forceFormRefresh = true;
            window.onbeforeunload = null;
            window.location.reload(true);
        })
        .always(function (data, textStatus, XmlHttpRequest) {
            if (typeof subGridName === 'string') {
                Xrm.Page.ui.controls.get(subGridName);
            }
            else
                subGridName.refresh();
        });
    }
    else {
        alert("There must be at least one service related to this offer !");
    }
}

function failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown) {
    var errorText = "";
    if (errorThrown.toString().toLowerCase().indexOf("access") > -1) {
        errorText = "Your browser setting is blocking the requests to the server. Contact the system administrator !"
        alert(errorText);
        return;
    }
    else {
        if (typeof (XmlHttpRequest.responseText) != "undefined") {
            try {
                errorText = jQuery.parseJSON(XmlHttpRequest.responseText);
            } catch (exType) {
                errorText = errorThrown;
            }
        }
        else
            errorText = errorThrown;
    }
    if (errorText.error == null) {
        if (typeof (errorText.Message) != "undefined")
            alert("Error !\n" + errorText.Message);
        else
            alert("Error !\n" + errorText);
    }
    else {
        alert("Error !\n" + errorText.error.message.value);
    }
}

function enableFormButton(buttonName) {
    var uId = Xrm.Page.context.getUserId();
    var roleArray = Xrm.Page.context.getUserRoles();
    //Modify button exception is included because account managers can modify a service
    //during the implementation and during implementation the order service form
    //is readonly for them.    
    if (getFormType() != FORM_TYPE_UPDATE && buttonName != modifyServiceButtonName)
        return false;
    if (getLookupEntityId("new_stepstatusid") == null) {
        //if the error is being generated for the first time and
        //step status control is available on the form
        if (!statusNullAlert && Xrm.Page.getAttribute("new_stepstatusid") != null) {

            alert("Could not get status information !");
            statusNullAlert = true;
        }
        return false;
    }

    if (defaultTeams == null) {
        var reqDefTeams = new JSonObject();
        reqDefTeams.ODataSetName("TeamSet");
        reqDefTeams.mSelect(["TeamId"]);
        reqDefTeams.mFilter(["IsDefault eq true"]);
        var defTeamInfo = reqDefTeams.RetrieveData();
        defaultTeams = defTeamInfo;
    }

    if (buttonConfig == null) {
        var reqBtnConfig = new JSonObject();
        reqBtnConfig.ODataSetName("new_buttonconfigurationSet");
        reqBtnConfig.mSelect(["new_Enabled", "new_name", "new_CheckSites"]);
        reqBtnConfig.mFilter(["new_EntityName eq '" + getEntityLogicalName() + "'",
                              "new_StatusNameId/Id eq guid'" + getLookupEntityId("new_stepstatusid") + "'",
                              "statecode/Value eq 0"]);
        var btnInfo = reqBtnConfig.RetrieveData();
        buttonConfig = btnInfo;
    }
    if (buttonConfig.length > 0) {
        var myConfig = $jq.grep(buttonConfig, function (n) {
            return n.new_name.toString().toLowerCase() == buttonName.toLowerCase()
        });

        var enabled = false;
        if (myConfig.length > 0)
            enabled = myConfig[0].new_Enabled;

        if (enabled) {
            if (userConfig == null) {
                var uPersonnalTeam = new JSonObject();
                uPersonnalTeam.ODataSetName("TeamMembershipSet");
                uPersonnalTeam.mSelect(["TeamId", "SystemUserId"]);
                //generally the account manager should be able to click any enabled button during whole operation
                //but the below code is written that account manager should be able to hit modify button during implementation                
                var amInfo = Xrm.Page.getAttribute("new_accountmanagerid");
                if (amInfo != null) {
                    if (amInfo.getValue() != null)
                        uPersonnalTeam.mFilter(["SystemUserId eq guid'" + uId + "' or SystemUserId eq guid'" + amInfo.getValue()[0].id + "'"]);
                    else
                        uPersonnalTeam.mFilter(["SystemUserId eq guid'" + uId + "'"]);
                }
                else
                    uPersonnalTeam.mFilter(["SystemUserId eq guid'" + uId + "'"]);

                var uCnf = uPersonnalTeam.RetrieveData();
                if (uCnf.length > 0)
                    userConfig = uCnf;
            }

            if (userConfig.length > 0) {
                var ownerInfo = getLookupEntityId("ownerid");
                var tempEnable = false;

                for (var oCnt = 0; oCnt < userConfig.length; oCnt++) {
                    if (defaultTeams.length > 0) {
                        var isDefaultTeam = $jq.grep(defaultTeams,
                                                function (n) {
                                                    return n.TeamId.toLowerCase().replace("{", "").replace("}", "") == userConfig[oCnt].TeamId.toLowerCase().replace("{", "").replace("}", "")
                                                });

                        if (isDefaultTeam != null && isDefaultTeam.length > 0)
                            continue;
                    }
                    if (userConfig[oCnt].SystemUserId.toLowerCase().replace("{", "").replace("}", "") == uId.toLowerCase().replace("{", "").replace("}", "")) {
                        currentUserConfig.push(userConfig[oCnt].TeamId.toLowerCase().replace("{", "").replace("}", ""));
                    }
                    else {
                        var myAMng = Xrm.Page.getAttribute("new_accountmanagerid");
                        if (myAMng != null) {
                            if (myAMng.getValue() != null) {
                                if (userConfig[oCnt].SystemUserId.toLowerCase().replace("{", "").replace("}", "") ==
                                 myAMng.getValue()[0].id.toLowerCase().replace("{", "").replace("}", "")) {
                                    amConfig.push(userConfig[oCnt].TeamId.toLowerCase().replace("{", "").replace("}", ""));
                                }
                            }
                        }
                    }
                }

                for (var oCnt = 0; oCnt < userConfig.length; oCnt++) {
                    //if current user is in Owner's teams
                    //if (userConfig[oCnt].TeamId.toLowerCase().replace("{", "").replace("}", "") == ownerInfo.toLowerCase().replace("{", "").replace("}", "")) {
                    if (currentUserConfig[oCnt] == ownerInfo.toLowerCase().replace("{", "").replace("}", "")) {
                        tempEnable = true;
                        break;
                    }
                }

                if (!tempEnable) {
                    for (var oCnt = 0; oCnt < currentUserConfig.length; oCnt++) {
                        if (amConfig.indexOf(currentUserConfig[oCnt]) > -1) {
                            tempEnable = true;
                            break;
                        }
                    }
                }

                if (!tempEnable) {
                    if (roleArray.indexOf(sysCheck) == -1 &&
                        roleArray.indexOf(sysCheck.toLowerCase()) == -1 &&
                        roleArray.indexOf(sysCheck.toUpperCase()) == -1)
                        enabled = false;
                    else
                        enabled = true;
                }
                else
                    enabled = true;
            }
        }
        if (enabled) {
            //check sites
            //enable when smt has the record. and the record doesnt have any sites.
            if (myConfig[0].new_CheckSites != null && myConfig[0].new_CheckSites == true) {
                enabled = hasSites();
            }
        }
        return enabled;
    }
    else
        return false;

    return false;
}

function validatePriceCheckFields(EntityId, LogicalEntityName, CmdProps) {
    var entId = "";
    if (Array.isArray(EntityId) && EntityId.length > 0) {
        entId = EntityId[0];
    }
    else
        entId = EntityId;

    var reqFieldList = new Array();
    var productId = getLookupEntityId("new_productid");
    var statusNameId = getLookupEntityId("new_stepstatusid");
    var getStepConfig = new JSonObject();
    getStepConfig.ODataSetName("new_stepconfigurationSet");
    getStepConfig.mSelect(["new_ControlName", "new_ControlType", "new_ControlVisibility",
                           "new_FieldRequirement", "new_FormState", "new_ProductId", "new_TabName"]);
    getStepConfig.mFilter(["new_name eq '" + LogicalEntityName + "'", "new_StatusNameId/Id eq guid'" + statusNameId + "'",
                           "(new_ProductId/Id eq null or new_ProductId/Id eq guid'" + productId + "')"]);

    var stepResults = getStepConfig.RetrieveData();
    if (stepResults.length == 0) {
        alert("Could not find the configuration for the given parameters !");
        return null;
    }
    for (var i in stepResults) {
        //is control a field
        if (stepResults[i].new_ControlType.Value == 1) {
            if (stepResults[i].new_FieldRequirement.Value == 1) {
                if (Xrm.Page.getAttribute(stepResults[i].new_ControlName).getValue() == null) {
                    Xrm.Page.getAttribute(stepResults[i].new_ControlName).setRequiredLevel("required");
                    reqFieldList.push(Xrm.Page.getControl(stepResults[i].new_ControlName).getLabel());
                }
            }
        }
    }
    if (reqFieldList.length > 0)
        Xrm.Page.data.entity.save();
}

function cCreateApprovalActivities(CmdProperties) {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var controls = CmdProperties.SourceControlId.split("|");
    var btnName = controls[controls.length - 1];

    $jq.ajax({
        type: "GET",
        url: helperServiceUrl + "CreateApprovalActivities",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        async: false,
        //crossDomain: true,
        data: { firmOfferId: Xrm.Page.data.entity.getId(), userid: currentUser, buttonName: btnName }
    }).done(function (data, textStatus, XmlHttpRequest) {
        if (data != null && typeof (data.d) != "undefined")
            parseRequiredFields(data.d);
        else {
            forceFormRefresh = true;
            window.onbeforeunload = null;
            window.location.reload(true);
        }
        //call open entity form method in global scripts
        //        Xrm.Page.getAttribute("new_approvalstatus").setValue(1);
        //        Xrm.Page.getAttribute("new_approvalstatus").setSubmitMode("always");
        //        Xrm.Page.data.entity.save();

    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function cDiscountRequest(EntityIds, firmOfferId, CmdProperties) {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (EntityIds.length > 0) {
        var controls = CmdProperties.SourceControlId.split("|");
        var btnName = controls[controls.length - 1];

        $jq.ajax({
            type: "GET",
            url: helperServiceUrl + "DiscountRequest",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            cache: false,
            async: false,
            //crossDomain: true,
            data: { FirmProductIds: JSON.stringify(EntityIds), FirmOfferId: firmOfferId, userId: currentUser, buttonName: btnName }
        }).done(function (data, textStatus, XmlHttpRequest) {
            //alert("Requested discount !");
            forceFormRefresh = true;
            window.onbeforeunload = null;
            window.location.reload(true);
        }).fail(function (XmlHttpRequest, textStatus, errorThrown) {
            failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
        })
    .always(function (data, textStatus, XmlHttpRequest) {
    });
    }
    else {
        alert("Please select at least one service !");
    }
}


function checkCopyResults(returnValues) {
    if (Array.isArray(returnValues)) {
        var recCnt = 0;
        var errorCnt = 0;
        for (recCnt = 0; recCnt < returnValues.length; recCnt++) {
            if (returnValues[recCnt] == "00000000-0000-0000-0000-000000000000")
                errorCnt++;
        }
        if (errorCnt != 0) {
            alert(errorCnt + " of the selected " + returnValues.length + " could not be copied !");
            return false;
        }
        else
            return true;
    }
    else {
        if (returnValues == "00000000-0000-0000-0000-000000000000") {
            alert("Selected record could not be copied !");
            return false;
        }
        else
            return true;
    }
}

function startDeactivation() {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var orderServiceId = Xrm.Page.data.entity.getId();
    var order = Xrm.Page.getAttribute("new_orderid").getValue();

    $jq.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        url: helperServiceUrl + "CreateDeactiveRecord",
        cache: false,
        async: false,
        data: { orderServiceId: orderServiceId, orderId: order[0].id, userId: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        alert("Deactivation started !");
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function modifyService() {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var circuitId = getCurrentEntityId();

    $jq.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        url: helperServiceUrl + "ModifyService",
        cache: false,
        async: false,
        data: { serviceId: circuitId, userId: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        forceFormRefresh = true;
        window.onbeforeunload = null;
        window.location.reload(true);
        Xrm.Utility.openEntityForm("quote", data.d, null);
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function modifyOrder() {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    if (getRequiredFields() == true) {
        var circuitId = getCurrentEntityId();

        $jq.ajax({
            type: "GET",
            contentType: "application/json; charset=utf-8",
            datatype: "json",
            url: helperServiceUrl + "ModifyOrder",
            cache: false,
            async: false,
            data: { orderId: circuitId, userId: currentUser }
        }).done(function (data, textStatus, XmlHttpRequest) {
            Xrm.Utility.openEntityForm("quote", data.d, null);
        })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
    }
}

function moveNext(CmdProperties, validateAttrs) {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var valOffer = true;
    if (validateAttrs != null)
        valOffer = validateAttrs;
    var controls = CmdProperties.SourceControlId.split("|");
    var btnName = controls[controls.length - 1];
    $jq.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        url: helperServiceUrl + "MoveNextStep",
        cache: false,
        async: false,
        data: { LogicalName: getEntityLogicalName(), EntityId: getCurrentEntityId(), buttonName: btnName, validateOffer: valOffer, userId: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        if (data != null && typeof (data.d) != "undefined")
            parseRequiredFields(data.d);
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function moveBiggestNext(CmdProperties) {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var controls = CmdProperties.SourceControlId.split("|");
    var btnName = controls[controls.length - 1];
    $jq.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        url: helperServiceUrl + "MoveForward",
        cache: false,
        async: false,
        data: { LogicalName: getEntityLogicalName(), EntityId: getCurrentEntityId(), buttonName: btnName, validateOffer: true, userId: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        if (data != null && typeof (data.d) != "undefined")
            parseRequiredFields(data.d);
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function moveForUsedButton(CmdProperties) {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var controls = CmdProperties.SourceControlId.split("|");
    var btnName = controls[controls.length - 1];
    $jq.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        url: helperServiceUrl + "MoveForUsedButton",
        cache: false,
        async: false,
        data: { LogicalName: getEntityLogicalName(), EntityId: getCurrentEntityId(), buttonName: btnName, validateOffer: true, userId: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        if (data != null && typeof (data.d) != "undefined")
            parseRequiredFields(data.d);
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function clearifyData(CmdProperties) {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var controls = CmdProperties.SourceControlId.split("|");
    var btnName = controls[controls.length - 1];
    $jq.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        url: helperServiceUrl + "MoveNextOwner",
        cache: false,
        async: false,
        data: { LogicalName: getEntityLogicalName(), EntityId: getCurrentEntityId(), nextOwnerId: "{00000000-0000-0000-0000-000000000000}",
            nextOwnerTypeName: "", buttonName: btnName, validateOffer: false, userId: currentUser
        }
    }).done(function (data, textStatus, XmlHttpRequest) {
        if (data != null && typeof (data.d) != "undefined")
            parseRequiredFields(data.d);
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function assignBackToAM(CmdProperties) {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var controls = CmdProperties.SourceControlId.split("|");
    var btnName = controls[controls.length - 1];
    $jq.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        url: helperServiceUrl + "AssignBackToAM",
        cache: false,
        async: false,
        data: { LogicalName: getEntityLogicalName(), EntityId: getCurrentEntityId(), buttonName: btnName, userId: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        forceFormRefresh = true;
        window.onbeforeunload = null;
        window.location.reload(true);
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function parseRequiredFields(clickResults) {
    if (clickResults == null)
        return;

    if (clickResults.status == BTN_CLICK_RESULT_COMPLETED) {
        if (clickResults.refreshPage) {
            forceFormRefresh = true;
            window.onbeforeunload = null;
            window.location.reload(true);
        }
    }

    if (clickResults.status == BTN_CLICK_RESULT_EMAILCREATED) {
        if (clickResults.EmailId != null && clickResults.EmailId != "00000000-0000-0000-0000-000000000000") {
            Xrm.Utility.openEntityForm("email", clickResults.EmailId, null)
        }
        if (clickResults.refreshPage) {
            forceFormRefresh = true;
            window.onbeforeunload = null;
            window.location.reload(true);
        }
    }

    if (clickResults.status == BTN_CLICK_RESULT_FIELDSREQUIRED) {
        var dCount = 0;
        var cMsg = "Required fields were not filled for these services :";
        var reqFields = clickResults.RequiredFields;
        for (dCount = 0; dCount < reqFields.length; dCount++) {
            if (reqFields[dCount].Value.length > 0)
                cMsg += "\n" + reqFields[dCount].Key;
        }
        cMsg += "\nWould you like to see more details ?";
        if (confirm(cMsg)) {
            var dMsg = "";
            for (dCount = 0; dCount < reqFields.length; dCount++) {
                if (reqFields[dCount].Value.length > 0) {
                    dMsg += reqFields[dCount].Key;
                    var moreInfo = reqFields[dCount].Value;
                    var fCount = 0;
                    for (fCount = 0; fCount < moreInfo.length; fCount++) {
                        dMsg += "\n     " + moreInfo[fCount];
                    }
                    dMsg += "\n";
                }
            }
            alert(dMsg);
        }
    }
}

function requestFromSupp() {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    $jq.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        url: helperServiceUrl + "RequestFromSupplier",
        cache: false,
        async: false,
        data: { LogicalName: getEntityLogicalName(), EntityId: getCurrentEntityId(), userId: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        if (data != null && typeof (data.d) != "undefined")
            parseRequiredFields(data.d);
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function requestApproval() {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    $jq.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        url: helperServiceUrl + "RequestApproval",
        cache: false,
        async: false,
        data: { LogicalName: getEntityLogicalName(), EntityId: getCurrentEntityId(), userId: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        if (data != null && typeof (data.d) != "undefined")
            parseRequiredFields(data.d);
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function getRequiredFields() {
    var allAttributes = Xrm.Page.data.entity.attributes.get();
    var emptyFields = "";
    for (var i in allAttributes) {
        var myattribute = Xrm.Page.data.entity.attributes.get(allAttributes[i].getName());
        var requiredLevel = myattribute.getRequiredLevel();
        if (requiredLevel == "required" && myattribute.getValue() == null) {

            emptyFields += Xrm.Page.ui.controls.get(myattribute.getName()).getLabel() + "\n";
        }
    }
    if (emptyFields != "") {
        alert("This fields are empty :\n" + emptyFields);
        return false;
    }
    else {
        return true;
    }
}

function sendOrderForm(customTerm) {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    $jq.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        url: helperServiceUrl + "SendOrderForm",
        cache: false,
        async: false,
        data: { LogicalName: getEntityLogicalName(), EntityId: getCurrentEntityId(), userId: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        if (data != null && typeof (data.d) != "undefined") {
            parseRequiredFields(data.d);
            forceFormRefresh = true;
            window.onbeforeunload = null;
            window.location.reload(true);
        }
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function CompleteNegotiation() {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    $jq.ajax({
        type: "GET",
        url: helperServiceUrl + "CompleteNegotiation",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        async: false,
        data: { LogicalName: getEntityLogicalName(), EntityId: getCurrentEntityId(), userId: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        if (data != null && typeof (data.d) != "undefined") {
            parseRequiredFields(data.d);
        }
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function CancelOffer() {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;

    if (!confirm("Are you sure you want to cancel the offer ?"))
        return;

    $jq.ajax({
        type: "GET",
        url: helperServiceUrl + "CancelOffer",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        async: false,
        data: { LogicalName: getEntityLogicalName(), EntityId: getCurrentEntityId(), userId: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        alert("Offer has been canceled.");
        if (data != null && typeof (data.d) != "undefined")
            parseRequiredFields(data.d);
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function startImplementation(CmdProperties) {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var controls = CmdProperties.SourceControlId.split("|");
    var btnName = controls[controls.length - 1];
    $jq.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        url: helperServiceUrl + "StartImplementation",
        cache: false,
        async: false,
        data: { LogicalName: getEntityLogicalName(), EntityId: getCurrentEntityId(), buttonName: btnName, userId: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        if (data != null && typeof (data.d) != "undefined")
            parseRequiredFields(data.d);
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function GenerateTermsTemplate(CmdProperties) {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var controls = CmdProperties.SourceControlId.split("|");
    var btnName = controls[controls.length - 1];
    $jq.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        url: helperServiceUrl + "GenerateTermsTemplate",
        cache: false,
        async: false,
        data: { LogicalName: getEntityLogicalName(), EntityId: getCurrentEntityId(), userId: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        if (data != null && typeof (data.d) != "undefined")
            parseRequiredFields(data.d);
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function RequestLegalApprove(CmdProperties) {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var controls = CmdProperties.SourceControlId.split("|");
    var btnName = controls[controls.length - 1];
    $jq.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        url: helperServiceUrl + "RequestLegalApprove",
        cache: false,
        async: false,
        data: { LogicalName: getEntityLogicalName(), EntityId: getCurrentEntityId(), buttonName: btnName, userId: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        if (data != null && typeof (data.d) != "undefined")
            parseRequiredFields(data.d);
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function UpdateFormSend() {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    var lglName = getEntityLogicalName();
    if (lglName == "new_supplierfirmoffer") {
        lglName = "new_supplierFirmOffer";
        return;
        //Internet explorer is asking to re-post the already posted data which causes 
        //double step forward. On supplier side this function is moved to the service level.
    }
    else if (lglName == "new_supplierbudgetaryoffers") {
        lglName = "new_supplierBudgetaryOffers";
        return;
        //Internet explorer is asking to re-post the already posted data which causes 
        //double step forward. On supplier side this function is moved to the service level.
    }
    else if (lglName == "salesorder")
        lglName = "SalesOrder";
    else if (lglName.indexOf("new_") > -1)
        lglName = lglName.toLowerCase();
    else
        lglName = lglName[0].toUpperCase() + lglName.slice(1);
    var entId = getCurrentEntityId();

    var formInfo = new JSonObject();
    formInfo.ODataSetName(lglName + "Set");

    var today = new Date();
    var sendDate = new Date(today.getFullYear(), today.getMonth(), today.getDate(), today.getHours(), today.getMinutes(), today.getSeconds());

    var offerEntity = {
        new_FormSendDate: sendDate
    };
    var result = formInfo.UpdateRecord(entId, offerEntity);
    if (result)
        Xrm.Page.getAttribute("new_formsenddate").setValue(sendDate);
    else
        throw new Error("Could not set offer send date information !");
}

function suspend(CmdProperties) {
    if (!WarnUserIfFormIsDirty())
        return;

    //Suspension management is moved to plugin level
    //BG - 21.12.2015

    //    var controls = CmdProperties.SourceControlId.split("|");
    //    var btnName = controls[controls.length - 1];
    //    $jq.ajax({
    //        type: "GET",
    //        contentType: "application/json; charset=utf-8",
    //        datatype: "json",
    //        url: helperServiceUrl + "Suspend",
    //        cache: false,
    //        async: false,
    //        data: { LogicalName: getEntityLogicalName(), EntityId: getCurrentEntityId(), buttonName: btnName, validateOffer: true, userId: currentUser }
    //    }).done(function (data, textStatus, XmlHttpRequest) {
    //        if (data != null && typeof (data.d) != "undefined")
    //            parseRequiredFields(data.d);
    //    })
    //    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
    //        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    //    })
    //    .always(function (data, textStatus, XmlHttpRequest) {
    //        //this method is equivalent to finally block in try catch finally block
    //    });

    Xrm.Page.getAttribute("new_reasonofsuspensionid").setRequiredLevel("required");
    Xrm.Page.getControl("new_reasonofsuspensionid").setDisabled(false);
    Xrm.Page.getAttribute("new_reasonofsuspensionid").setSubmitMode("always");
    Xrm.Page.getAttribute("new_amsuspended").setValue(true);
    Xrm.Page.getAttribute("new_pmsuspended").setValue(false);
    Xrm.Page.data.entity.save();
}

function pmsuspend(CmdProperties) {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    //Suspension management is moved to plugin level
    //BG - 21.12.2015

    //    var controls = CmdProperties.SourceControlId.split("|");
    //    var btnName = controls[controls.length - 1];
    //    $jq.ajax({
    //        type: "GET",
    //        contentType: "application/json; charset=utf-8",
    //        datatype: "json",
    //        url: helperServiceUrl + "PMSuspend",
    //        cache: false,
    //        async: false,
    //        data: { LogicalName: getEntityLogicalName(), EntityId: getCurrentEntityId(), buttonName: btnName, validateOffer: false, userId: currentUser }
    //    }).done(function (data, textStatus, XmlHttpRequest) {
    //        if (data != null && typeof (data.d) != "undefined")
    //            parseRequiredFields(data.d);
    //    })
    //    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
    //        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    //    })
    //    .always(function (data, textStatus, XmlHttpRequest) {
    //        //this method is equivalent to finally block in try catch finally block
    //    });

    Xrm.Page.getAttribute("new_reasonofsuspensionid").setRequiredLevel("required");
    Xrm.Page.getControl("new_reasonofsuspensionid").setDisabled(false);
    Xrm.Page.getAttribute("new_reasonofsuspensionid").setSubmitMode("always");
    Xrm.Page.getAttribute("new_amsuspended").setValue(false);
    Xrm.Page.getAttribute("new_pmsuspended").setValue(true);
    Xrm.Page.data.entity.save();
}

function CollectFromSuppliers(CmdProperties) {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var controls = CmdProperties.SourceControlId.split("|");
    var btnName = controls[controls.length - 1];

    $jq.ajax({
        type: "GET",
        url: helperServiceUrl + "CollectFromSuppliers",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        async: false,
        //crossDomain: true,
        data: { firmOfferId: Xrm.Page.data.entity.getId(), userid: currentUser, buttonName: btnName }
    }).done(function (data, textStatus, XmlHttpRequest) {
        //call open entity form method in global scripts
        if (data != null && typeof (data.d) != "undefined")
            parseRequiredFields(data.d);
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function travelInHistory(entityId, parentTypeName, parentId) {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    $jq.ajax({
        type: "GET",
        url: helperServiceUrl + "TravelInHistory",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        async: false,
        //crossDomain: true,
        data: { HistoryId: entityId, entityTypeName: parentTypeName, entityId: parentId, userid: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        //call open entity form method in global scripts
        if (data != null && typeof (data.d) != "undefined") {
            parseRequiredFields(data.d);
            forceFormRefresh = true;
            window.onbeforeunload = null;
            window.parent.location.reload(true);
        }
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function hasSites() {
    var entName = getEntityLogicalName();
    var entId = getCurrentEntityId();
    var lkpName = "";

    if (entName == "new_budgetaryoffer") {
        lkpName = "new_BudgetaryOfferId";
    }
    if (entName == "quote") {
        lkpName = "new_FirmOfferId";
    }

    if (lkpName == "") //dont display button
        return false;

    if (entId != null) {
        var SiteInfo = new JSonObject();
        SiteInfo.ODataSetName("new_siteSet");
        SiteInfo.mSelect(["new_SiteNumber"]);
        SiteInfo.mFilter([lkpName + "/Id eq guid'" + entId + "'",
                          "statuscode/Value eq 1"]);
        var sInfo = SiteInfo.RetrieveData();
        if (sInfo.length > 0) {
            //dont display button
            return false;
        }
        else {
            //display button
            return true;
        }
    }
    else {
        //dont display button
        return false;
    }
}

function startDisconnection() {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var disconnection = Xrm.Page.getAttribute("new_disconnection").getValue();

    if (disconnection == false) {
        Xrm.Page.getAttribute("new_disconnection").setValue(true);
        Xrm.Page.data.entity.attributes.get("new_disconnection").fireOnChange();
        Xrm.Page.getAttribute("new_disconnection").setSubmitMode("always");
        Xrm.Page.data.entity.save();
    }
    else {
        Xrm.Page.data.entity.save();
    }
}

function sendDeactivationToAM() {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var getDeactivationId = Xrm.Page.data.entity.getId();

    $jq.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        url: helperServiceUrl + "sendDeactivationToAM",
        cache: false,
        async: false,
        data: { deactivationId: getDeactivationId, userId: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        alert("Deactivation record is asgineed to AM!");
        forceFormRefresh = true;
        window.onbeforeunload = null;
        window.location.reload(true);
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function assignBackToPmLeader() {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var getDeactivationId = Xrm.Page.data.entity.getId();

    $jq.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        url: helperServiceUrl + "assignBackToPmLeader",
        cache: false,
        async: false,
        data: { deactivationId: getDeactivationId, userId: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        alert("Deactivation record is asgineed to PM Leader!");
        forceFormRefresh = true;
        window.onbeforeunload = null;
        window.location.reload(true);
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function homeAssignToProjectManager(EntityTypeName, EntityIds) {
    debugger;
    var isHome = true;
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (EntityIds.length > 0) {
        if (EntityIds.length > 15) {
            alert("You can select maximum 15 records !");
            return;
        }

        for (var i = 0; i < EntityIds.length; i++) {
            assignToProjectManager(EntityIds[i]);
            //            console.log("Item: " + EntityIds[i]);
        }

    }
    else {
        alert("Please select at least one service !");
    }
}

function assignToProjectManager(deactivationId,isHome) {
    debugger;
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (isHome) {
        if (!WarnUserIfFormIsDirty())
            return;
    }
       
    var getDeactivationId = "";
    if (deactivationId == "")
        getDeactivationId = Xrm.Page.data.entity.getId();
    else
        getDeactivationId = deactivationId;
    //var projectManagerId = getLookupEntityId("new_projectmanagerid");

    //    if (projectManagerId != null) {
    $jq.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        url: helperServiceUrl + "assignToPmManager",
        cache: false,
        async: false,
        data: { deactivationId: getDeactivationId, userId: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        alert("Deactivation record is assigned to Project Manager!");
        forceFormRefresh = true;
        window.onbeforeunload = null;
        window.location.reload(true);
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
    
    //    else {
    //        alert("Please enter project manager information first !");
    //        Xrm.Page.ui.controls.get("new_projectmanagerid").setDisabled(false);
    //        Xrm.Page.getAttribute("new_projectmanagerid").setRequiredLevel("required");
    //    }
}

function ValidateAddressViaGoogle(formattedAddress) {
    $jq.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        url: helperServiceUrl + "ValidateAddressViaGoogle",
        cache: false,
        async: false,
        data: { formattedAddress: formattedAddress }
    }).done(function (data, textStatus, XmlHttpRequest) {
        return data.d;
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function thirdPartyFinalization() {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var getDeactivationId = Xrm.Page.data.entity.getId();

    var projectManagerId = getLookupEntityId("new_projectmanagerid");
    if (projectManagerId != null) {
        $jq.ajax({
            type: "GET",
            contentType: "application/json; charset=utf-8",
            datatype: "json",
            url: helperServiceUrl + "thirdPartyFinalization",
            cache: false,
            async: false,
            data: { deactivationId: getDeactivationId, projectManagerId: projectManagerId, userId: currentUser }
        }).done(function (data, textStatus, XmlHttpRequest) {
            alert("Deactivation record is assigned to Project Manager!");
            forceFormRefresh = true;
            window.onbeforeunload = null;
            window.location.reload(true);
        })
        .fail(function (XmlHttpRequest, textStatus, errorThrown) {
            failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
        })
        .always(function (data, textStatus, XmlHttpRequest) {
            //this method is equivalent to finally block in try catch finally block
        });
    }
    else {
        alert("Please enter project manager information first !");
        Xrm.Page.ui.controls.get("new_projectmanagerid").setDisabled(false);
        Xrm.Page.getAttribute("new_projectmanagerid").setRequiredLevel("required");
    }
}

function assignToBilling() {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    var getDeactivationId = Xrm.Page.data.entity.getId();

    var cancellationDate = Xrm.Page.getAttribute("new_srvcancellationdate").getValue();
    if (cancellationDate != null) {
        $jq.ajax({
            type: "GET",
            contentType: "application/json; charset=utf-8",
            datatype: "json",
            url: helperServiceUrl + "assignToBilling",
            cache: false,
            async: false,
            data: { deactivationId: getDeactivationId, userId: currentUser }
        }).done(function (data, textStatus, XmlHttpRequest) {
            alert("Deactivation record is assigned to Billing!");
            forceFormRefresh = true;
            window.onbeforeunload = null;
            window.location.reload(true);
        })
        .fail(function (XmlHttpRequest, textStatus, errorThrown) {
            failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
        })
        .always(function (data, textStatus, XmlHttpRequest) {
            //this method is equivalent to finally block in try catch finally block
        });
    }
    else {
        alert("Please enter date of service cancellation information first !");
        Xrm.Page.ui.controls.get("new_srvcancellationdate").setDisabled(false);
        Xrm.Page.getAttribute("new_srvcancellationdate").setRequiredLevel("required");
    }
}

function transactionSendAgainToSAP() {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    $jq.ajax({
        type: "GET",
        url: helperServiceUrl + "TransactionSendAgainToSap",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        async: false,
        //crossDomain: true,
        data: { entityId: Xrm.Page.data.entity.getId(), userid: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        if (data != null && data.d != null && data.d != "00000000-0000-0000-0000-000000000000") {
            alert("Transaction was sent to SAP !");
            Xrm.Utility.openEntityForm("new_sapmonitoringlog", data.d, null);
        }
        else {
            alert("An error occurred sending again to SAP Service !");
        }
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}

function packageSendAgainToSAP() {
    if (forceFormRefresh) {
        alert("This form requires to be refreshed before any button click. Hit Control + F5 before clicking any buttons !");
        return;
    }
    if (!WarnUserIfFormIsDirty())
        return;
    $jq.ajax({
        type: "GET",
        url: helperServiceUrl + "PackageSendAgainToSap",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        async: false,
        //crossDomain: true,
        data: { entityId: Xrm.Page.data.entity.getId(), userid: currentUser }
    }).done(function (data, textStatus, XmlHttpRequest) {
        if (data != null && data.d != null && data.d != "00000000-0000-0000-0000-000000000000") {
            alert("Package was sent to SAP !");

            Xrm.Utility.openEntityForm("new_sapmonitoringpackage", data.d, null);
        }
        else {
            alert("An error occurred sending again to SAP Service !");
        }
    })
    .fail(function (XmlHttpRequest, textStatus, errorThrown) {
        failedAjaxRequest(XmlHttpRequest, textStatus, errorThrown);
    })
    .always(function (data, textStatus, XmlHttpRequest) {
        //this method is equivalent to finally block in try catch finally block
    });
}
