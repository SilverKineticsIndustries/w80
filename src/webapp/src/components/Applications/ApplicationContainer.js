import React, { useState } from 'react';
import { useSelector } from 'react-redux';
import ApplicationEdit from './ApplicationEdit';
import ApplicationView from './ApplicationView';
import { selectApplicationById } from '../../store/applications/selectors';

export default function ApplicationContainer({id, allowEdit=false,
    allowDelete=false, allowArchive=false,allowUnarchive=false, allowReject=false, allowExpand=false,
    allowStateChange=false, allowAccept=false })
{
    const [editable, setEditable] = useState();
    const application = useSelector(state => selectApplicationById(state, id));

    const showInEditMode = () => {
        return application.isNew || editable === id;
    }

    return (
        <div>
            {!showInEditMode() && <ApplicationView id={id} onBeginEdit={(id) => setEditable(id)} allowEdit={allowEdit}
                allowDelete={allowDelete} allowArchive={allowArchive} allowUnarchive={allowUnarchive}
                allowReject={allowReject} allowExpand={allowExpand} allowStateChange={allowStateChange}
                allowAccept={allowAccept} />}
            {showInEditMode() && <ApplicationEdit id={id} onCompleteEdit={() => setEditable('')} />}
        </div>
    )
}