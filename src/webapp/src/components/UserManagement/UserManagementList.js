import React, { useEffect, useState, useContext, memo, useCallback } from 'react'
import { createUseStyles } from 'react-jss'
import edit from '../../assets/edit.png';
import add from '../../assets/add.png';
import remove from '../../assets/remove.png';
import checkmark from '../../assets/checkmark.png';
import invitationCode from '../../assets/invitationCode.png';
import emailVerificationCode from '../../assets/emailVerificationCode.png';
import ToolButton from '../../common/ToolButton';
import UserManagementEdit from './UserManagementEdit';
import { printUtcShortDate } from '../../helpers/dates';
import CopyToClipboard from '../../common/CopyToClipboard';
import { sortById } from '../../helpers/common';
import ModalWrapper from '../../common/ModalWrapper';
import ValidationPanel from '../../common/ValidationPanel';
import { getUsers, deactivateUser } from '../../services/userService';
import { useTranslation } from 'react-i18next';
import { generateInvitationCode, resendEmailVerificationCode } from '../../services/autheticationService';
import InfoPanel from '../../common/InfoPanel';
import { StatusContext } from '../../App';
import { apiDirectDecorator, apiDecoratorOptions } from '../../helpers/api';

const styles = createUseStyles({
    tableContainer: {
        maxHeight: "50vh",
        overflow: "scroll",
        paddingRight: "8px",
        maxWidth: "70vw",
        marginBottom: "10px",
        marginTop: "10px"
    },
    formHeader: {
        textAlign: "center",
        borderBottom: "1px solid var(--semi-dark)",
        paddingBottom: "6px"
    }
})

const UserManagementList = ({onClose}) =>
{
    const classes = styles();
    const [users, setUsers] = useState([]);
    const [editingUser, setEditingUser] = useState();
    const [refreshRequired, setRefreshRequired] = useState(0);
    const [validationErrors, setValidationErrors] = useState([]);
    const [invitationCodeValue, setInvitationCodeValue] = useState(null);
    const { t } = useTranslation(null, { keyPrefix: "user-management"});
    const { setLoading, setServerErrorMessage } = useContext(StatusContext);
    const [showConfirmationEmailResentPanel, setShowConfirmationEmailResentPanel] = useState(null);

    useEffect((e) => {
        apiDirectDecorator(
            async () => await getUsers(),
            apiDecoratorOptions(
                { setLoading, setServerErrorMessage },
                (data) =>setUsers(data.sort(sortById))
            ))();
    },[refreshRequired, setLoading, setServerErrorMessage]);

    const onAddClick = () => {
        setEditingUser({
            role: "User",
            culture: "en-US",
            timezone: "America/New_York"
        });
    }

    const onEditClick = (user) => {
        setEditingUser(user);
    }

    const onPostUpdate = (e) => {
        setRefreshRequired(new Date().getTime());
        setEditingUser();
        setInvitationCodeValue(e.invitationCode);
    }

    const onDeactivateClick = useCallback((e, user) => {
        apiDirectDecorator(
            async () => await deactivateUser(user.id),
            apiDecoratorOptions(
                { setLoading, setServerErrorMessage },
                () => setRefreshRequired(new Date().getTime()),
                (validationErrors) => setValidationErrors(validationErrors),
                e.target
            ))();
    },[setLoading, setServerErrorMessage])

    const onGenerateInvitationCode = useCallback((e, user) => {
        apiDirectDecorator(
            async () => await generateInvitationCode(user.email),
            apiDecoratorOptions(
                { setLoading, setServerErrorMessage },
                (data) => setInvitationCodeValue(data.code),
                (validationErrors) => setValidationErrors(validationErrors),
                e.target
            ))();
    },[setLoading, setServerErrorMessage]);

    const onResendEmailVerificationCode = useCallback((e, user) => {
        apiDirectDecorator(
            async () => await resendEmailVerificationCode(user.email),
            apiDecoratorOptions(
                { setLoading, setServerErrorMessage },
                () => setShowConfirmationEmailResentPanel(true),
                (validationErrors) => setValidationErrors(validationErrors),
                e.target
            ))();
    },[setLoading, setServerErrorMessage]);

    return (
        <ModalWrapper>
            <fieldset className="no-border">
                <div className={classes.formHeader}>
                    {t("title")}
                </div>
                {invitationCodeValue && <InfoPanel header={t("user-invitation-code")} message={invitationCodeValue} showCopyToClipboard={true} onHide={() => setInvitationCodeValue(null)} />}
                {showConfirmationEmailResentPanel && <InfoPanel message={t("confirmation-sent")} onHide={() => setShowConfirmationEmailResentPanel(false)} />}
                <div>
                    <div className={classes.tableContainer}>
                        {!editingUser &&
                            <div className={classes.toolbar}>
                                <ToolButton onClick={onAddClick} disabled={editingUser}
                                    img={add} tooltip={t("add-new-user")} data-test="user-management-user-add" />
                            </div>
                        }
                        {editingUser && <UserManagementEdit user={editingUser} onPostUpdate={onPostUpdate} onCancel={() => setEditingUser(null)} />}
                        <table>
                            <thead>
                                <tr>
                                    <th/>
                                    <th>{t("email")}</th>
                                    <th>{t("nickname")}</th>
                                    <th>{t("role")}</th>
                                    <th>{t("req-invitation-code")}</th>
                                    <th>{t("email-confirmed")}</th>
                                    <th>{t("created-date")}</th>
                                    <th>{t("created-by")}</th>
                                    <th>{t("updated-date")}</th>
                                    <th>{t("updated-by")}</th>
                                    <th>{t("deactivated-date")}</th>
                                    <th>{t("deactivated-by")}</th>
                                </tr>
                            </thead>
                            <tbody>
                            {
                                (users || []).map((user) =>
                                    <tr key={user.id}>
                                        <td className="nowrap">
                                            <ToolButton onClick={(e) => onDeactivateClick(e, user)}
                                                confirmationMessage={t("deactivation-confirm", { email: user.email })}
                                                disabled={editingUser} img={remove} tooltip={t("deactivate-user")}
                                                data-test="user-management-deactivate-user" />
                                            <ToolButton onClick={() => onEditClick(user)}
                                                disabled={editingUser} img={edit} tooltip={t("edit-user")}
                                                data-test="user-management-edit-user" />
                                        </td>
                                        <td className="nowrap">
                                            {user.email}
                                            <CopyToClipboard value={user.email} />
                                        </td>
                                        <td>
                                            {user.nickname}
                                        </td>
                                        <td>
                                            {user.role}
                                        </td>
                                        <td className="center">
                                            {user.mustActivateWithInvitationCode && <span>
                                                <img src={checkmark} height={10} width={10} className='grid-checkbox-img' alt={t("must-activate-invitation-code")} />
                                                <ToolButton onClick={(e) => onGenerateInvitationCode(e, user)} disabled={editingUser}
                                                    img={invitationCode} width={24} height={24} tooltip={t("generate-invitation-code")}
                                                    data-test="user-management-generate-invitation" />
                                                </span>
                                            }
                                        </td>
                                        <td className="center">
                                            {user.isEmailOwnershipConfirmed &&
                                                <img src={checkmark} height={10} width={10} className='grid-checkbox-img' alt={t("email-ownership-confirmed")} />
                                            }
                                            {!user.isEmailOwnershipConfirmed &&
                                                <ToolButton onClick={(e) => onResendEmailVerificationCode(e, user)}
                                                    disabled={editingUser} img={emailVerificationCode} width={24} height={24}
                                                    tooltip={t("resend-email-confirmation")}
                                                    data-test="user-management-resend-verification" />
                                            }
                                        </td>
                                        <td className="nowrap">
                                            {printUtcShortDate(user.createdUTC)}
                                        </td>
                                        <td>
                                            {user.createdBy}
                                        </td>
                                        <td className="nowrap">
                                            {printUtcShortDate(user.updatedUTC)}
                                        </td>
                                        <td>
                                            {user.updatedBy}
                                        </td>
                                        <td className="nowrap">
                                            {printUtcShortDate(user.deactivatedUTC)}
                                        </td>
                                        <td>
                                            {user.deactivatedBy}
                                        </td>
                                    </tr>
                                )
                            }
                            </tbody>
                        </table>
                    </div>
                </div>
                {validationErrors?.length > 0 && <ValidationPanel data={validationErrors} />}
                <div className="editor-buttons-container" disabled={editingUser}>
                    <button className="editor-button" onClick={onClose} data-test="user-management-close">{t("close")}</button>
                </div>
            </fieldset>
        </ModalWrapper>
    )
}

export default memo(UserManagementList);