import React, { useState, memo, useCallback } from 'react'
import { createUseStyles } from 'react-jss'
import ContactEdit from './ContactEdit';
import edit from '../../assets/edit.png';
import add from '../../assets/add.png';
import remove from '../../assets/remove.png';
import ToolButton from '../../common/ToolButton';
import CopyToClipboard from '../../common/CopyToClipboard';
import { useTranslation } from 'react-i18next';

const styles = createUseStyles({
    wrapper: {
        maxHeight: "200px",
        overflow: "scroll",
        paddingRight: "8px"
    },
    table: {
        width: "100%"
    }
})

const displayContactRole = (t, role) => {
    if (role === 'Recruiter')
        return t("contact-role-recruiter");
    else if (role === 'HiringManager')
        return t("contact-role-hiring-manager");
    else if (role === 'HumanResources')
        return t("contact-role-human-resources");
    else if (role === 'TeamMember')
        return t("contact-role-team-member");
    else if (role === 'Other')
        return t("contact-role-other");
}

const ContactList = ({contacts, onUpdate, allowEdit=true, dataTestInstanceId=""}) =>
{
    const classes = styles();
    const { t } = useTranslation(null, { keyPrefix: "application" });
    const [editingContact, setEditingContact] = useState();

    const onAddClick = useCallback(() => {
        const newContact = {};
        newContact.type = 'Email';
        newContact.role = 'Recruiter';
        newContact.seqNo = (contacts || []).length > 0 ? Math.max( ...contacts.map((x) => x.seqNo)) + 1 : 1;
        setEditingContact(newContact);
    }, [setEditingContact, contacts]);

    const onEditClick = useCallback((contact) => {
        setEditingContact(contact);
    }, [setEditingContact]);

    const onUpsertClick = useCallback((updatedContact) => {
        var items = structuredClone(contacts);
        var item = items.find((x) => x.seqNo === updatedContact.seqNo);
        if (!item)
            items.push(updatedContact);
        else
            Object.assign(item, updatedContact);

        setEditingContact();
        onUpdate(items);
    }, [contacts, setEditingContact, onUpdate]);

    const onDeleteClick = useCallback((deletedContact) => {
        var items = structuredClone(contacts || []);
        const updated = items.filter(x => x.seqNo !== deletedContact.seqNo);
        onUpdate(updated);
    },[contacts, onUpdate]);

    return (
        <div className={classes.wrapper}>
            {allowEdit && !editingContact && <ToolButton onClick={onAddClick} disabled={editingContact} img={add} tooltip={t("add-new-contact")}/>}
            {allowEdit && editingContact && <ContactEdit contact={editingContact} onUpdate={onUpsertClick} onCancel={() => setEditingContact(null)} dataTestInstanceId={dataTestInstanceId} />}
            <table className={classes.table}>
                <thead>
                    <tr>
                        {allowEdit && <th/>}
                        <th>{t("contact-name")}</th>
                        <th>{t("contact-role")}</th>
                        <th>{t("contact-parameter")}</th>
                    </tr>
                </thead>
                <tbody>
                    {
                        (contacts || []).sort((a,b) => b.seqNo - a.seqNo).map((contact, idx) =>
                            <tr key={idx}>
                                {allowEdit &&
                                    <td>
                                        <ToolButton onClick={() => onDeleteClick(contact)} disabled={editingContact}
                                            img={remove} tooltip={t("delete-contact")} data-test={`contact-list-${dataTestInstanceId}-${idx}-delete`} />
                                        <ToolButton onClick={() => onEditClick(contact)} disabled={editingContact}
                                            img={edit} tooltip={t("edit-contact")} data-test={`contact-list-${dataTestInstanceId}-${idx}-edit`}/>
                                    </td>
                                }
                                <td>
                                    <span data-test={`contact-list-${dataTestInstanceId}-${idx}-name`}>
                                        {contact.contactName}
                                    </span>
                                </td>
                                <td>
                                    <span data-test={`contact-list-${dataTestInstanceId}-${idx}-role`}>
                                        {displayContactRole(t, contact.role)}
                                    </span>
                                </td>
                                <td>
                                    <span data-test={`contact-list-${dataTestInstanceId}-${idx}-parameter`}>
                                        {contact.contactParameter}
                                    </span>
                                    {contact.type === 'Email' &&
                                        <CopyToClipboard value={contact.contactParameter} />
                                    }
                                </td>
                            </tr>
                        )
                    }
                </tbody>
            </table>
        </div>
    )
}

export { displayContactRole };
export default memo(ContactList);