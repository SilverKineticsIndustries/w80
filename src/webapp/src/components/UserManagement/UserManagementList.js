import React, { useEffect, useState, useContext } from 'react'
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

export default function UserManagementList({onClose})
{
    const classes = styles();
    const { t } = useTranslation();
    const [users, setUsers] = useState([]);
    const statusContext = useContext(StatusContext);
    const [editingUser, setEditingUser] = useState();
    const [refreshRequired, setRefreshRequired] = useState(0);
    const [validationErrors, setValidationErrors] = useState([]);
    const [invitationCodeValue, setInvitationCodeValue] = useState(null);
    const [showConfirmationEmailResentPanel, setShowConfirmationEmailResentPanel] = useState(null);

    useEffect((e) => {
        apiDirectDecorator(
            async () => await getUsers(),
            apiDecoratorOptions(
                statusContext,
                (data) =>setUsers(data.sort(sortById))
            ))();
    },[refreshRequired]);

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

    const onDeactivateClick = (e, user) => {
        apiDirectDecorator(
            async () => await deactivateUser(user.id),
            apiDecoratorOptions(
                statusContext,
                () => setRefreshRequired(new Date().getTime()),
                (validationErrors) => setValidationErrors(validationErrors),
                e.target
            ))();
    }

    const onPostUpdate = (e) => {
        setRefreshRequired(new Date().getTime());
        setEditingUser();
        setInvitationCodeValue(e.invitationCode);
    }

    const onGenerateInvitationCode = (e, user) => {
        apiDirectDecorator(
            async () => await generateInvitationCode(user.email),
            apiDecoratorOptions(
                statusContext,
                (data) => setInvitationCodeValue(data.code),
                (validationErrors) => setValidationErrors(validationErrors),
                e.target
            ))();
    }

    const onResendEmailVerificationCode = (e, user) => {
        apiDirectDecorator(
            async () => await resendEmailVerificationCode(user.email),
            apiDecoratorOptions(
                statusContext,
                () => setShowConfirmationEmailResentPanel(true),
                (validationErrors) => setValidationErrors(validationErrors),
                e.target
            ))();
    }

    return (
        <ModalWrapper>
            <fieldset className="no-border">
                <div className={classes.formHeader}>
                    {t("user-management.title")}
                </div>
                {invitationCodeValue && <InfoPanel header={t("user-management.user-invitation-code")} message={invitationCodeValue} showCopyToClipboard={true} onHide={() => setInvitationCodeValue(null)} />}
                {showConfirmationEmailResentPanel && <InfoPanel message={t("user-management.confirmation-sent")} onHide={() => setShowConfirmationEmailResentPanel(false)} />}
                <div>
                    <div className={classes.tableContainer}>
                        {!editingUser &&
                            <div className={classes.toolbar}>
                                <ToolButton onClick={onAddClick} disabled={editingUser}
                                    img={add} tooltip={t("user-management.add-new-user")} data-test="user-management-user-add" />
                            </div>
                        }
                        {editingUser && <UserManagementEdit user={editingUser} onPostUpdate={onPostUpdate} onCancel={() => setEditingUser(null)} />}
                        <table>
                            <thead>
                                <tr>
                                    <th/>
                                    <th>{t("user-management.email")}</th>
                                    <th>{t("user-management.nickname")}</th>
                                    <th>{t("user-management.role")}</th>
                                    <th>{t("user-management.req-invitation-code")}</th>
                                    <th>{t("user-management.email-confirmed")}</th>
                                    <th>{t("user-management.created-date")}</th>
                                    <th>{t("user-management.created-by")}</th>
                                    <th>{t("user-management.updated-date")}</th>
                                    <th>{t("user-management.updated-by")}</th>
                                    <th>{t("user-management.deactivated-date")}</th>
                                    <th>{t("user-management.deactivated-by")}</th>
                                </tr>
                            </thead>
                            <tbody>
                            {
                                (users || []).map((user) =>
                                    <tr key={user.id}>
                                        <td className="nowrap">
                                            <ToolButton onClick={(e) => onDeactivateClick(e, user)}
                                                confirmationMessage={t("user-management.deactivation-confirm", { email: user.email })}
                                                disabled={editingUser} img={remove} tooltip={t("user-management.deactivate-user")}
                                                data-test="user-management-deactivate-user" />
                                            <ToolButton onClick={() => onEditClick(user)}
                                                disabled={editingUser} img={edit} tooltip={t("user-management.edit-user")}
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
                                                <img src={checkmark} height={10} width={10} className='grid-checkbox-img' alt={t("user-management.must-activate-invitation-code")} />
                                                <ToolButton onClick={(e) => onGenerateInvitationCode(e, user)} disabled={editingUser}
                                                    img={invitationCode} width={24} height={24} tooltip={t("user-management.generate-invitation-code")}
                                                    data-test="user-management-generate-invitation" />
                                                </span>
                                            }
                                        </td>
                                        <td className="center">
                                            {user.isEmailOwnershipConfirmed &&
                                                <img src={checkmark} height={10} width={10} className='grid-checkbox-img' alt={t("user-management.email-ownership-confirmed")} />
                                            }
                                            {!user.isEmailOwnershipConfirmed &&
                                                <ToolButton onClick={(e) => onResendEmailVerificationCode(e, user)}
                                                    disabled={editingUser} img={emailVerificationCode} width={24} height={24}
                                                    tooltip={t("user-management.resend-email-confirmation")}
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
                    <button className="editor-button" onClick={onClose} data-test="user-management-close">{t("user-management.close")}</button>
                </div>
            </fieldset>
        </ModalWrapper>
    )
}