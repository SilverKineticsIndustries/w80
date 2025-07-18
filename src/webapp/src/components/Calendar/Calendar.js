import dayjs from "dayjs";
import "dayjs/locale/de";
import { useState, useContext, memo } from "react";
import { UserContext } from "../../App";
import { Calendar as BigCalendar, dayjsLocalizer } from "react-big-calendar";
import { useSelector } from "react-redux";
import { useSearchParams } from "react-router-dom";
import "react-big-calendar/lib/css/react-big-calendar.css";
import Appointment from "./Appointment";
import {getLocaleFromCulture} from "../../helpers/common";
import { selectCalendarAppointmentsForApplications} from "../../store/applications/selectors";

const localizer = dayjsLocalizer(dayjs)

const Calendar = () =>
{
    const { currentUser } = useContext(UserContext);
    const culture = currentUser.culture;

    dayjs.locale(getLocaleFromCulture(culture));
    const [searchParams] = useSearchParams();
    const id = searchParams.get("id");
    const appointments = useSelector(selectCalendarAppointmentsForApplications);

    const initialAppointment = !id ? null : appointments.find((x) => x.id.toLowerCase() === id.toLowerCase());
    const [editingAppointment, setEditingAppointment] = useState(initialAppointment);

    const handleSelectSlot = ({ start, end }) => {
        setEditingAppointment((state) => ({...state, 'startDateTimeUTC': start, 'endDateTimeUTC': end}));
    }

    const handleSelectAppointment = (selectedAppointment) => {
        const appointment = appointments.find((x) => x.id.toLowerCase() === selectedAppointment.id.toLowerCase());
        setEditingAppointment(appointment);
    }

    return (
        <div>
            <div>
                <BigCalendar
                  localizer={localizer}

                  /* Calendar requires a property called 'title' */
                  events={appointments.map((x) => { return {
                        id: x.id,
                        title: x.description,
                        endDateTimeUTC: x.endDateTimeUTC,
                        startDateTimeUTC: x.startDateTimeUTC}})}

                  startAccessor="startDateTimeUTC"
                  endAccessor="endDateTimeUTC"
                  onSelectEvent={handleSelectAppointment}
                  onSelectSlot={handleSelectSlot}
                  culture={culture}
                  selectable
                  style={{ height: 500 }}
                />
            </div>
        <div>
            {editingAppointment && <Appointment selectedAppointment={editingAppointment} onClose={setEditingAppointment}/>}
        </div>
    </div>
  );
}

export default memo(Calendar);