import React, { memo } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { Tooltip } from 'react-tooltip'
import { v4 as uuidv4 } from 'uuid';

const LeftMenuItem = ({path, img, tooltip}) =>
{
    const uniqueid = uuidv4();
    const navigator = useNavigate();
    const location = useLocation();
    const selectedClass = location.pathname.toLowerCase() === path.toLowerCase() ? "left-menu-item-selected" : "";

    return (
        <React.Fragment>
            <li data-tooltip-id={uniqueid} className={`left-menu-item ${selectedClass}`} onClick={() => navigator(path)}>
                <img src={img} height={50} width={50} alt={tooltip} />
            </li>
            <Tooltip id={uniqueid} delayShow="100" place="top">{tooltip}</Tooltip>
        </React.Fragment>
    )
}

export default memo(LeftMenuItem);

