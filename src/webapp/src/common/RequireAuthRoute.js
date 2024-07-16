import { useContext } from "react";
import { UserContext } from "../App";
import { Navigate } from "react-router-dom";

export default function RequireAuthRoute({ children }) {
    const { currentUser } = useContext(UserContext);
    return currentUser ? children : <Navigate to="/" replace />;
}