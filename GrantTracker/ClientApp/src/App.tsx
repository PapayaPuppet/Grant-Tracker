import { createContext, useEffect, useState } from 'react'
import { useLocation, Location } from 'react-router-dom'
import { Container } from 'react-bootstrap'
import { QueryClient, useQuery } from '@tanstack/react-query'

import Breadcrumb from 'components/Breadcrumb'
import MainNavigation from 'components/MainNavigation'
import { PayrollYear } from 'Models/PayPeriod'

import paths from 'utils/routing/paths'
import appRoutes, { RenderRoutes } from 'utils/routing'
import { IdentityClaim, User as User } from 'utils/authentication'
import api, { AxiosIdentityConfig } from 'utils/api'


export const App = (): JSX.Element => {
  const location: Location = useLocation()
  const [user, setUser] = useState<User>(new User(null)) //move into appcontext
  const [isLoading, setIsLoading] = useState<boolean>(false)
  const { data: payrollYears, refetch: refetchPayrollYears } = useQuery({
    queryKey: ['payrollYear'],
    queryFn: () => api.get('dropdown/payrollYear').then(res => res.data.map(py => PayrollYear.toViewModel(py)))
  }, new QueryClient())

  const appContextValue = { refetchPayrollYears, data: { payrollYears} };

  const breadcrumbItems: string[] = location.pathname
    .split('/')
    .filter(item => item && item.length < 15)
    .map(item => `${item.charAt(0).toUpperCase()}${item.slice(1)}`)

  //this could all be cleaned up.
  const handleOrgYearChange = () => {
    //reassign user on change to redisplay page
    setUser(Object.assign(Object.create(Object.getPrototypeOf(user)), user))
    AxiosIdentityConfig.setOrganizationYear(user.organization.guid, user.year.guid, user.organizationYearGuid, user.userGuid)
  }

  useEffect(() => {
    setIsLoading(true)
    User
      .initUserAsync()
      .then(res => {
        setUser(res)
        AxiosIdentityConfig.initialize(res.organization.guid, res.year.guid, res.organizationYearGuid, user.userGuid)
      })
      .finally(() => { setIsLoading(false)})
  }, [])

  if (!isLoading && (!user || user.claim == IdentityClaim.Unauthenticated))
    return (
      <div> Please allow the page a moment to load. 
        You may be unauthenticated, refresh the page or fill out an <a href='https://forms.office.com/r/0Hq5fsxHze'>issue report</a> with your badge number and organization if the issue persists.
      </div>
    )
  else if (isLoading)
      return <p>...Loading user information.</p>

  return (
    <div className=''>
      <MainNavigation
        key={user.badgeNumber}
        paths={paths}
        user={user}
        orgYearChange={() => handleOrgYearChange()}
      />
      <Container
        className='d-flex flex-column align-items-center w-100 mx-5'
        style={{ paddingTop: '6.5rem', minWidth: '95vw' }}
      >
        <AppContext.Provider value={appContextValue}>
          <RenderRoutes 
            routes={appRoutes} 
            user={user} 
            breadcrumbs={
              <Breadcrumb
                items={breadcrumbItems}
                activeItem={breadcrumbItems[breadcrumbItems.length - 1]}
              />
            } 
          />
        </AppContext.Provider>
      </Container>
    </div>
  )
}

interface AppContext {
  refetchPayrollYears: () => undefined,
  data: {
    payrollYears: PayrollYear[]
  }
}

export const AppContext = createContext<AppContext>({
  refetchPayrollYears: () => undefined,
  data: {
    payrollYears: []
  }
})