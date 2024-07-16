import React, { memo } from 'react';
import { createUseStyles } from 'react-jss';
import { printLocalizedShortDate } from '../../helpers/dates';
import { useNavigate } from 'react-router-dom';
import calendar from '../../assets/calendar.png';

const styles = createUseStyles({
    wrapper: {
        cursor: "pointer",
        width: "360px",
        whiteSpace: "nowrap",
        overflow: "hidden",
        textOverflow: "ellipsis",
        verticalAlign: "top",
        '&:hover ': {
            textDecoration: "underline"
        }
    }
})

const CalendarLink = ({appointment}) => {

    const classes = styles();
    const navigator = useNavigate();

    const openCalendarItem = () => {
        navigator('/calendar?id=' + encodeURIComponent(appointment.id));
    }

    return (
        <React.Fragment>
        {appointment &&
            <React.Fragment>
                <div className={classes.wrapper}>
                    <img src={calendar} width="20px" height="20px" style={{marginRight: '8px'}} alt='Calendar' />
                    <a onClick={() => openCalendarItem()}>
                        {printLocalizedShortDate(appointment.startDateTimeUTC)} - {appointment.description}
                    </a>
                </div>
            </React.Fragment>
        }
        </React.Fragment>
    )
}

export default memo(CalendarLink);