import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { useIntl } from 'react-intl';
import { Navigate, useNavigate } from 'react-router-dom';

import { TextInput } from '@/components/TextInput';
import { ActionButton } from '@/components/ActionButton';
import { useAppDispatch, useAppSelector } from '@/store';
import { loginAsync, clearError } from '@/store/slices/authSlice';
import Logo from '@/assets/icons/svg/logo.svg';
import './Auth.scss';

interface LoginFormData {
  email: string;
  password: string;
  tenantCode: string;
}

export const Login: React.FC = () => {
  const { formatMessage } = useIntl();
  const navigate = useNavigate();
  const dispatch = useAppDispatch();

  const authState = useAppSelector((state) => state.auth);
  const isAuthenticated = authState?.isAuthenticated ?? false;
  const loginLoading = authState?.loginLoading ?? false;
  const error = authState?.error ?? null;

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>();

  useEffect(() => {
    if (isAuthenticated) {
      navigate('/dashboard', { replace: true });
    }
  }, [isAuthenticated, navigate]);

  useEffect(() => {
    return () => {
      dispatch(clearError());
    };
  }, [dispatch]);

  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }

  const onSubmit = async (data: LoginFormData) => {
    dispatch(
      loginAsync({
        email: data.email,
        password: data.password,
        tenantCode: data.tenantCode,
      })
    );
  };

  return (
    <div className="login-page">
      <div className="login-container">
        <div className="login-header">
          <div className="login-brand">
            <span className="login-logo"><Logo /></span>
            <h1>SmartWMS</h1>
          </div>
          <p>
            {formatMessage({
              id: 'login.page.subtitle',
              defaultMessage: 'Warehouse Management System',
            })}
          </p>
        </div>

        <form className="login-form" onSubmit={handleSubmit(onSubmit)}>
          <TextInput
            label={formatMessage({
              id: 'login.form.email',
              defaultMessage: 'Email',
            })}
            name="email"
            type="email"
            register={register('email', {
              required: formatMessage({
                id: 'login.error.emailRequired',
                defaultMessage: 'Email is required',
              }),
              pattern: {
                value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                message: formatMessage({
                  id: 'login.error.emailRequired',
                  defaultMessage: 'Invalid email address',
                }),
              },
            })}
            errorText={errors.email?.message}
            required
          />

          <TextInput
            label={formatMessage({
              id: 'login.form.password',
              defaultMessage: 'Password',
            })}
            name="password"
            type="password"
            register={register('password', {
              required: formatMessage({
                id: 'login.error.passwordRequired',
                defaultMessage: 'Password is required',
              }),
            })}
            errorText={errors.password?.message}
            required
          />

          <TextInput
            label={formatMessage({
              id: 'login.form.tenantCode',
              defaultMessage: 'Tenant Code',
            })}
            name="tenantCode"
            type="text"
            register={register('tenantCode', {
              required: formatMessage({
                id: 'login.error.tenantRequired',
                defaultMessage: 'Tenant Code is required',
              }),
            })}
            errorText={errors.tenantCode?.message}
            required
          />

          {error && <div className="login-error">{error}</div>}

          <ActionButton
            type="submit"
            label={formatMessage({
              id: 'login.action.submit',
              defaultMessage: 'Sign In',
            })}
            loading={loginLoading}
            disabled={loginLoading}
            variant="success"
            className="login-button"
          />
        </form>
      </div>
    </div>
  );
};

export default Login;
