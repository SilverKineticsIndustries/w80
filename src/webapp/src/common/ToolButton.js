import React, { useCallback, memo } from 'react';
import { Tooltip } from 'react-tooltip'
import { v4 as uuidv4 } from 'uuid';

const ToolButton = ({onClick, img, tooltip, disabled=false, confirmationMessage='', height='30px', width='30px', className='', dataTest=''}) =>
{
    const uniqueid = uuidv4();
    const onClickHandler = useCallback((e) => {
        e.preventDefault();
        if (confirmationMessage) {
            if (window.confirm(confirmationMessage)) {
                onClick(e);
            }
        }
        else
            onClick(e);
    }, [confirmationMessage, onClick])

    return (
        <>
            <button data-tooltip-id={uniqueid} className={`toolbar-button ${uniqueid} ${className}`}
                disabled={disabled} onClick={(e) => onClickHandler(e)} data-test={dataTest}>
                <img src={img} tooltip={tooltip} style={{ height: height, width: width }} alt={tooltip} />
            </button>
            <Tooltip id={uniqueid} delayShow="100" place="top">{tooltip}</Tooltip>
        </>
    )
}

export default memo(ToolButton);