import React, { useState, memo } from 'react'
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
        return t("application.contact-role-recruiter");
    else if (role === 'HiringManager')
        return t("application.contact-role-hiring-manager");
    else if (role === 'HumanResources')
        return t("application.contact-role-human-resources");
    else if (role === 'TeamMember')
        return t("application.contact-role-team-member");
    else if (role === 'Other')
        return t("application.contact-role-other");
}

const ContactList = ({contacts, onUpdate, allowEdit=true}) =>
{
    const classes = styles();
    const { t } = useTranslation();
    const [editingContact, setEditingContact] = useState();

    const onAddClick = () => {
        let newContact = {};
        newContact.type = 'Email';
        newContact.role = 'Recruiter';
        newContact.seqNo = contacts.length > 0 ? Math.max( ...contacts.map((x) => x.seqNo)) + 1 : 1;
        setEditingContact(newContact);
    }

    const onEditClick = (contact) => {
        setEditingContact(contact);
    }

    const onUpsertClick = (updatedContact) => {
        var items = structuredClone(contacts);
        var item = items.find((x) => x.seqNo === updatedContact.seqNo);
        if (!item)
            items.push(updatedContact);
        else
            Object.assign(item, updatedContact);

        setEditingContact();
        onUpdate(items);
    }

    const onDeleteClick = (deletedContact) => {
        var items = structuredClone(contacts);
        const updated = items.filter(x => x.seqNo !== deletedContact.seqNo);
        onUpdate(updated);
    }

    const onCancelEdit = () => {
        setEditingContact();
    }

    return (
        <div className={classes.wrapper}>
            {allowEdit && !editingContact && <ToolButton onClick={onAddClick} disabled={editingContact} img={add} tooltip={("t.applications.add-new-contact")}/>}
            {allowEdit && editingContact && <ContactEdit contact={editingContact} onUpdate={onUpsertClick} onCancel={onCancelEdit} />}
            <table className={classes.table}>
                <thead>
                    <tr>
                        {allowEdit && <th/>}
                        <th>{t("application.contact-name")}</th>
                        <th>{t("application.contact-role")}</th>
                        <th>{t("application.contact-parameter")}</th>
                    </tr>
                </thead>
                <tbody>
                    {
                        (contacts || []).sort((a,b) => b.seqNo - a.seqNo).map((contact, idx) =>
                            <tr key={idx}>
                                {allowEdit &&
                                    <td>
                                        <ToolButton onClick={() => onDeleteClick(contact)} disabled={editingContact} img={remove} tooltip={t("application.delete-contact")}/>
                                        <ToolButton onClick={() => onEditClick(contact)} disabled={editingContact} img={edit} tooltip={t("application.edit-contact")}/>
                                    </td>
                                }
                                <td>
                                    {contact.contactName}
                                </td>
                                <td>
                                    {displayContactRole(t, contact.role)}
                                </td>
                                <td>
                                    {contact.contactParameter}
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